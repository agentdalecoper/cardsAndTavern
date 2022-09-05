// using Leopotam.Ecs;
//
// namespace Client
// {
//     internal class CardInitSystem : IEcsInitSystem
//     {
//         private GameContext gameContext;
//         private EcsWorld ecsWorld;
//
//         public void Init()
//         {
//             foreach (Card card in gameContext.cardsPlayer)
//             {
//                 EcsEntity cardEntity = ecsWorld.NewEntity();
//                 cardEntity.Get<Card>();
//
//                 gameContext.cardEntitiesPlayer.Add(cardEntity);
//             }
//
//             foreach (Card card in gameContext.cardsEnemy)
//             {
//                 EcsEntity cardEntity = ecsWorld.NewEntity();
//                 cardEntity.Get<Card>();
//
//                 gameContext.cardEntitiesEnemy.Add(cardEntity);
//             }
//         }
//     }
// }