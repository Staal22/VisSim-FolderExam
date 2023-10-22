// using System.Collections;
// using System.Collections.Generic;
// using Unity.Entities;
// using Unity.Mathematics;
// using Unity.Transforms;
// using UnityEngine;
//
// public partial class  MeshRenderingSystem : SystemBase
// {
//     protected override void OnUpdate()
//     {
//         if (TerrainManager.Instance == null)
//             return;
//         if (TerrainManager.Instance.renderPointCloud == false)
//             return;
//         
//         var mesh = TerrainManager.Instance.instanceMesh;
//         var material = TerrainManager.Instance.instanceMaterial;
//         
//         Entities
//             .WithName("MeshRenderingSystem")
//             .ForEach((in DynamicBuffer<LinkedEntityGroup> entityGroup) =>
//             {
//                 var count = entityGroup.Length;
//                 var batchCount = Mathf.CeilToInt(count / 1023f);
//         
//                 for (int i = 0; i < batchCount; i++)
//                 {
//                     var startIndex = i * 1023;
//                     var endIndex = math.min((i + 1) * 1023, count);
//                     var range = endIndex - startIndex;
//         
//                     var batchMatrices = new Matrix4x4[range];
//         
//                     for (var j = startIndex; j < endIndex; j++)
//                     {
//                         var entity = entityGroup[j].Value;
//                         var ltw = EntityManager.GetComponentData<LocalToWorld>(entity);
//                         batchMatrices[j - startIndex] = ltw.Value;
//                     }
//                     Graphics.DrawMeshInstanced(mesh, 0, material, batchMatrices, batchMatrices.Length);
//                 }
//             })
//             .WithoutBurst() 
//             .Run();
//     }
// }
