// using System.Collections;
// using NUnit.Framework;
// using UnityEngine;
// using UnityEngine.TestTools;
// using GoPath.Navigation;

// namespace GoPath.Tests
// {
//     public class PathfindingTests
//     {
//         private GridManager gridManager;
//         private GameObject walkableArea;

//         [UnitySetUp]
//         public IEnumerator SetUp()
//         {
//             walkableArea = new GameObject("WalkableArea");
//             var collider = walkableArea.AddComponent<BoxCollider2D>();
//             collider.size = new Vector2(10f, 10f);

//             var gameObject = new GameObject("GridManager");
//             gridManager = gameObject.AddComponent<GridManager>();
//             gridManager.Initialize(walkableArea);

//             yield return null;
//         }

//         [UnityTest]
//         public IEnumerator FindPath_WithValidPoints_ReturnsPath()
//         {
//             // Arrange
//             Vector2 start = new Vector2(0f, 0f);
//             Vector2 end = new Vector2(1f, 1f);

//             yield return null;

//             // Act
//             var path = gridManager.FindPath(start, end);
            
//             // Assert
//             Assert.That(path, Is.Not.Null);
//             Assert.That(path.Count, Is.GreaterThan(0));
//         }

//         [UnityTest]
//         public IEnumerator MovementBlocker_WhenBlocked_PreventMovement()
//         {
//             // Arrange
//             Assert.That(MovementBlocker.IsBlocked, Is.False);
            
//             // Act
//             MovementBlocker.AddBlock();
//             yield return null;

//             // Assert
//             Assert.That(MovementBlocker.IsBlocked, Is.True);
            
//             // Cleanup
//             MovementBlocker.RemoveBlock();
//             Assert.That(MovementBlocker.IsBlocked, Is.False);
//         }

//         [UnityTearDown]
//         public IEnumerator TearDown()
//         {
//             if (walkableArea != null)
//                 Object.DestroyImmediate(walkableArea);
//             if (gridManager != null)
//                 Object.DestroyImmediate(gridManager.gameObject);
//             yield return null;
//         }
//     }
// }
