// using System.Linq;
// using Leopotam.Ecs;
// using UnityEngine;
//
// namespace Client
// {
//     internal class CardChooseTurnSystem : IEcsRunSystem
//     {
//         private GameContext gameContext;
//         private const float INIT_DELAY = 5f;
//         private float delay = 0;
//
//         public void Run()
//         {
//             if (delay <= 0f)
//             {
//                 delay = INIT_DELAY;
//             }
//             else
//             {
//                 delay -= Time.deltaTime;
//                 return;
//             }
//
//             if (gameContext.cardEntitiesEnemy.Any(c => c.Has<TurnEnded>()))
//             {
//                 EcsEntity cardEntity
//                     = gameContext.cardEntitiesEnemy.First(c => !c.Has<TurnEnded>());
//                 cardEntity.Get<Turn>();
//             }
//         }
//     }
//
//     internal struct Turn
//     {
//     }
//
//
//     internal struct TurnEnded
//     {
//     }
// }