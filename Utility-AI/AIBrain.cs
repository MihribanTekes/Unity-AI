using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TL.Core;
using TL.UI;

namespace TL.UtilityAI
{
    public class AIBrain : MonoBehaviour
    {
        public bool finishedDeciding { get; set; }
        public bool finishedExecutingBestAction { get; set; }

        public Action bestAction { get; set; }
        private NPCController npc;

        [SerializeField] private Billboard billBoard;
        [SerializeField] private Action[] actionsAvailable;

        // Start is called before the first frame update
        void Start()
        {
            npc = GetComponent<NPCController>();
            finishedDeciding = false;
            finishedExecutingBestAction = false;
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void DecideBestAction()
        {
            finishedExecutingBestAction = false;

            float score = 0f;
            int nextBestActionIndex = 0;
            for (int i = 0; i < actionsAvailable.Length; i++)
            {
                if (ScoreAction(actionsAvailable[i]) > score)
                {
                    nextBestActionIndex = i;
                    score = actionsAvailable[i].score;
                }
            }

            bestAction = actionsAvailable[nextBestActionIndex];
            bestAction.SetRequiredDestination(npc);

            finishedDeciding = true;
            billBoard.UpdateBestActionText(bestAction.Name);
        }

        public float ScoreAction(Action action)
        {
            float score = 1f;
            for (int i = 0; i < action.considerations.Length; i++)
            {
                float considerationScore = action.considerations[i].ScoreConsideration(npc);
                score *= considerationScore;

                if (score == 0)
                {
                    action.score = 0;
                    return action.score; 
                }
            }

            float originalScore = score;
            float modFactor = 1 - (1 / action.considerations.Length);
            float makeupValue = (1 - originalScore) * modFactor;
            action.score = originalScore + (makeupValue * originalScore);

            return action.score;
        }


    }
}
