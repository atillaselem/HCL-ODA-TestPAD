using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HCL_ODA_TestPAD.HCL.CAD.Math.API;
using ODA.Kernel.TD_RootIntegrated;
using ODA.Visualize.TV_Visualize;

namespace HCL_ODA_TestPAD.HCL.Visualize.Extensions
{
    internal static class TvExtensions
    {
        internal static IEnumerable<T> Select<T>(this OdTvEntity entity, Func<OdTvGeometryDataIterator, T> action)
        {
            using var iterator = entity.getGeometryDataIterator();

            if (iterator != null && action != null)
            {
                while (!iterator.done())
                {
                    yield return action(iterator);

                    iterator.step();
                }
            }
        }

        internal static IEnumerable<TResult> Select<TState, TResult>(
            this TState initialState,
            Func<TState, TResult> resultSelector) where TState : OdTvIterator

        {
            ArgumentNullException.ThrowIfNull(initialState);
            ArgumentNullException.ThrowIfNull(resultSelector);

            for (; !initialState.done(); initialState.step())
            {
                yield return resultSelector(initialState);
            }

            initialState.Dispose();
        }

        internal static CadMatrix3D GetSubItemPathMatrix(this OdTvSubItemPath path, CadMatrix3D transformMatrix, bool includeGeometries)
        {
            using var entityArray = path.entitiesIds();
            if (entityArray.Count == 0)
            {
                return transformMatrix;
            }

            if (includeGeometries)
            {
                using var geometryDataArray = path.geometryDatasIds();
                foreach (var geometryId in geometryDataArray)
                {
                    if (geometryId.getType() == OdTv_OdTvGeometryDataType.kSubEntity)
                    {
                        using var entity = geometryId.openObject();
                        using var modelingMatrix = entity.getModelingMatrix();
                        using var preMultiBy = transformMatrix.PreMultiplyWith(modelingMatrix);
                    }
                    else
                    {
                        using var entity = geometryId.openObject();
                        using var modelingMatrix = entity.getModelingMatrix();
                        using var preMultiBy = transformMatrix.PreMultiplyWith(modelingMatrix);
                    }
                }
            }

            for (var i = 0; i < entityArray.Count; i++)
            {
                var entityId = entityArray[i];
                ApplyEntityTransform(transformMatrix, entityId);
            }

            using var modelId = entityArray.Last().getOwnerModel();
            if (modelId.isNull())
            {
                return transformMatrix;
            }

            {
                using var model = modelId.openObject(OdTv_OpenMode.kForRead);
                using var matrix = model.getModelingMatrix();
                using var preMultiBy = transformMatrix.PreMultiplyWith(matrix);
            }
            return transformMatrix;
        }

        public static void ApplyEntityTransform(CadMatrix3D transformMatrix, OdTvEntityId entityId)
        {
            if (entityId.getType() == OdTvEntityId_EntityTypes.kEntity)
            {
                using var entity = entityId.openObject(OdTv_OpenMode.kForRead);
                using var modelingMatrix = entity.getModelingMatrix();
                using var preMultiBy = transformMatrix.PreMultiplyWith(modelingMatrix);
            }
            else
            if (entityId.getType() == OdTvEntityId_EntityTypes.kInsert)
            {
                using var entity = entityId.openObjectAsInsert(OdTv_OpenMode.kForRead);
                using var modelingMatrix = entity.getBlockTransform();
                using var preMultiBy = transformMatrix.PreMultiplyWith(modelingMatrix);
            }
        }

        internal static CadMatrix3D GetTransformWithSubEntities(this OdTvGeometryDataId id, CadMatrix3D parentBlockTransform, bool transformParent = true)
        {
            var xfm = CadMatrix3D.Identity;
            if (id == null)
            {
                return xfm;
            }
            using var data = id.openObject();
            using var currTransform = data.getModelingMatrix();
            using var identity = CadMatrix3D.Identity;
            // incorporate geometry-level matrix first(lowest-level)
            if (currTransform.IsNotEqual(identity))
            {
                using var preMultiBy = xfm.PreMultiplyWith(currTransform);
            }

            // travel up all sub-entities stack/multiply their matrices
            var parentId = data.getParentSubEntity();

            while (!parentId.isNull())
            {
                if (parentId.getType() == OdTv_OdTvGeometryDataType.kUndefinied)
                {
                    break;
                }
                using var parentSubEntity = parentId.openObject();
                using var subTransform = parentSubEntity.getModelingMatrix();
                using var newMultBy = xfm.PreMultiplyWith(subTransform);
                parentId.Dispose();
                parentId = parentSubEntity.getParentSubEntity();
            }

            parentId?.Dispose();
            using var newMultBy1 = xfm.PreMultiplyWith(parentBlockTransform);
            if (!transformParent)
            {
                return xfm;
            }
            using var parentEntityId = data.getParentTopLevelEntity();
            using var parentEntity = parentEntityId.openObject(OdTv_OpenMode.kForRead);
            using var parentTransform = parentEntity.getModelingMatrix();
            using var model = parentEntityId.getOwnerModel();
            using var modelOpen = model.openObject();
            using var modelTransform =
                !model.isNull() ? modelOpen.getModelingMatrix() : CadMatrix3D.GeIdentity;

            //  include also the model-transformation matrix (top-top-level)
            using var newMultBy2 = xfm.PreMultiplyWith(parentTransform);
            using var newMultBy3 = xfm.PreMultiplyWith(modelTransform);

            return xfm;
        }

        internal static bool IsNullOrEmpty(this OdTvEntity odTvEntity) => odTvEntity == null || odTvEntity.isEmpty();

        internal static void OverrideSubEntityColor(this OdTvEntity en, OdTvColorDef color)
        {
            if (en == null || en.isEmpty())
            {
                return;
            }
            using var iterator = en.getGeometryDataIterator();
            while (!iterator.done())
            {
                using var geometryData = iterator.getGeometryData();
                iterator.step();

                if (geometryData == null || geometryData.isNull() || !geometryData.isValid())
                {
                    continue;
                }

                using var geometryDataOpen = geometryData.openObject();
                if (geometryDataOpen == null)
                {
                    continue;
                }
                var type = geometryDataOpen.getType();
                if (type != OdTv_OdTvGeometryDataType.kSubEntity)
                {
                    continue;
                }
                using var subEntity = geometryData.openAsSubEntity(OdTv_OpenMode.kForRead);
                subEntity.setColor(color);
            }
        }
    }

}
