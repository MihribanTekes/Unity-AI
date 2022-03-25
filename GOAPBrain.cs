using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GOAPBrain : MonoBehaviour
{
    BaseGoal[] Goals;
    BaseAction[] Actions;

    BaseGoal ActiveGoal;
    BaseAction ActiveAction;

    public string DebugInfo_ActiveGoal => ActiveGoal != null ? ActiveGoal.GetType().Name : "None";
    public string DebugInfo_ActiveAction => ActiveAction != null ? $"{ActiveAction.GetType().Name}{ActiveAction.GetDebugInfo()}" : "None";
    public int NumGoals => Goals.Length;

    public string DebugInfo_ForGoal(int index)
    {
        return Goals[index].GetDebugInfo();
    }

    void Awake()
    {
        Goals = GetComponents<BaseGoal>();
        Actions = GetComponents<BaseAction>();        
    }

    void Start()
    {
        if (AIDebugger.Instance != null)
            AIDebugger.Instance.Register(this);
    }

    void OnDestroy()
    {
        if (AIDebugger.Instance != null)
            AIDebugger.Instance.Deregister(this);
    }

    void Update()
    {
        
        for (int goalIndex = 0; goalIndex < Goals.Length; ++goalIndex)
            Goals[goalIndex].PreTick();

        RefreshPlan();

        if (ActiveGoal != null)
        {
            ActiveGoal.Tick();

            
            if (ActiveAction.HasFinished)
            {
                ActiveGoal.GoToSleep();
                ActiveGoal = null;
                ActiveAction = null;
            }
        }
    }

    void RefreshPlan()
    {
        
        BaseGoal bestGoal = null;
        BaseAction bestAction = null;
        for (int goalIndex = 0; goalIndex < Goals.Length; ++goalIndex)
        {
            var candidateGoal = Goals[goalIndex];

            
            if (!candidateGoal.CanRun)
                continue;

            
            if (bestGoal != null && bestGoal.Priority > candidateGoal.Priority)
                continue;

            
            BaseAction bestActionForCandidateGoal = null;
            for (int actionIndex = 0; actionIndex < Actions.Length; ++actionIndex)
            {
                var candidateAction = Actions[actionIndex];

                
                if (!candidateAction.CanSatisfy(candidateGoal))
                    continue;

                
                if (bestActionForCandidateGoal != null && candidateAction.Cost() > bestActionForCandidateGoal.Cost())
                    continue;

                bestActionForCandidateGoal = candidateAction;
            }

            
            if (bestActionForCandidateGoal != null)
            {
                bestGoal = candidateGoal;
                bestAction = bestActionForCandidateGoal;
            }
        }

        
        if (bestGoal == ActiveGoal && bestAction == ActiveAction)
            return;

        
        if (bestGoal == null)
        {
            if (ActiveGoal != null)
                ActiveGoal.GoToSleep();

            ActiveGoal = null;
            ActiveAction = null;
            return;
        }

        
        if (bestGoal != ActiveGoal)
        {
            if (ActiveGoal != null)
                ActiveGoal.GoToSleep();

            bestGoal.Wakeup();
        }

        
        ActiveGoal = bestGoal;
        ActiveAction = bestAction;
        ActiveGoal.SetAction(ActiveAction);
    }
}
