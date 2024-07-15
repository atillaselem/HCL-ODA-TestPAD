using HCL_ODA_TestPAD.Functional.Extensions;
using HCL_ODA_TestPAD.HCL.CAD.Math.API;
using ODA.Visualize.TV_Visualize;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using HCL_ODA_TestPAD.HCL.Visualize.Extensions;
using HCL_ODA_TestPAD.ViewModels.Base;

namespace HCL_ODA_TestPAD.HCL.Visualize
{
    public record struct EntityHandle(ulong HandleId)
    {
        public ulong HandleId { get; internal set; } = HandleId;
    }
    public class TvModel
    {
        private readonly OdTvModelId _tvModelId;
        private readonly List<ulong> _hiddenEntityList = new();
        public TvModel([NotNull] OdTvModelId tvModelId)
        {
            _tvModelId = tvModelId ?? throw new ArgumentNullException(nameof(tvModelId));
        }

        internal T GetValue<T>(Func<OdTvModel, T> action)
        {
            using var model = _tvModelId.openObject();
            return action(model);
        }
        public T SetValue<T>(Func<OdTvModel, T> action)
        {
            using var db = _tvModelId.openObject(OdTv_OpenMode.kForWrite);

            return action(db);
        }

        public string Name()
        {
            return GetValue(model => model.getName());
        }
        public void SetName(string name)
        {
            SetValue(m => m.setName(name));
        }

        public OdTvEntityId AppendEntity(string name)
        {
            return SetValue(m => m.appendEntity(name));
        }
        public OdTvEntity OpenEntity(string name, OdTv_OpenMode mode = OdTv_OpenMode.kForWrite)
        {
            using var entityId = SetValue(m => m.appendEntity(name));
            return entityId.openObject(mode);
        }

        public void RemoveEntity(ulong handleId)
        {
            SetValue(m =>
            {
                using var id = m.findEntity(handleId);
                return m.removeEntity(id);
            });
        }

        internal void Show(EntityHandle handle) => Show(handle.HandleId);
        private void Show(ulong handleId)
        {
            SetValue(m =>
            {
                using var id = m.findEntity(handleId);
                return m.unHide(id);
            });
        }
        internal void Hide(EntityHandle handle) => Hide(handle.HandleId);
        private void Hide(ulong handleId)
        {
            SetValue(m =>
            {
                using var id = m.findEntity(handleId);
                return m.hide(id);
            });
        }

        public void Hide()
        {
            ChangeVisibility(false);
        }
        public void Show()
        {
            ChangeVisibility(true);
        }
        private void ChangeVisibility(bool showHide)
        {
            using var modelPtr = _tvModelId.openObject(OdTv_OpenMode.kForWrite);
            using var entityIterator = modelPtr.getEntitiesIterator();
            using var visible = new OdTvVisibilityDef(showHide);

            for (; !entityIterator.done(); entityIterator.step())
            {
                using var entId = entityIterator.getEntity();
                using var entPtr = entId.openObject(OdTv_OpenMode.kForWrite);
                entPtr.setVisibility(visible);
            }

            _hiddenEntityList.ForEach(Hide);
        }

        public void UpdateScaleAtLocation(CadPoint3D location, double scaleFactor)
        {
            SetValue(m =>
            {
                using var modelingMatrix = GetValue(model => model.getModelingMatrix());
                using var scalingMatrix = CadMatrix3D.ScaleWithCenterPoint(scaleFactor, location);
                using var scaledModelingMatrix = modelingMatrix.preMultBy(scalingMatrix);
                return m.setModelingMatrix(scaledModelingMatrix, true);
            });
        }
        public void UpdateScaleEntityHandlesAtLocation(double scaleFactor, params ulong[] entityIds)
        {
            SetValue(m =>
            {
                entityIds.ForEach(handleId =>
                {
                    var entId = m.findEntity(handleId);
                    using var entityObj = entId.openObject(OdTv_OpenMode.kForWrite);
                    using var modelingMatrix = entityObj.getModelingMatrix();
                    using var scalingMatrix = CadMatrix3D.ScaleWith(scaleFactor);
                    using var scaledModelingMatrix = modelingMatrix.preMultBy(scalingMatrix);
                    entityObj.setModelingMatrix(scaledModelingMatrix);
                });
                return OdTvResult.tvOk;
            });
        }
        public void UpdateScale(double scaleFactor)
        {
            SetValue(m =>
            {
                using var modelingMatrix = GetValue(model => model.getModelingMatrix());
                using var scalingMatrix = CadMatrix3D.ScaleWith(scaleFactor);
                using var scaledModelingMatrix = modelingMatrix.preMultBy(scalingMatrix);
                return m.setModelingMatrix(scaledModelingMatrix, true);
            });
        }

        //Fast Transform does not work for individual entity transformation.
        public void UpdateEntityDirection(OdTvGsViewId view, params ulong[] entityHandles)
        {
            SetValue(m =>
            {
                entityHandles.ForEach(handleId =>
                {
                    var isCadRotated = view.IsCADRotated();
                    if (isCadRotated)
                    {
                        Show(handleId);
                        var entityId = m.findEntity(handleId);
                        using var entityObj = entityId.openObject(OdTv_OpenMode.kForWrite);
                        using var modelingMatrix = entityObj.getModelingMatrix();
                        using var eyeToWorldMatrix = view.EyeToWorldMatrix();
                        using var originVector = CadVector3D.Default;
                        using var eyeToWorldTranslatedMatrix = eyeToWorldMatrix.SetTranslation(originVector);
                        //OK : yon degistirken entity scale carpmadan model kücülüp büyüyor.
                        using var scaleMatrix = CadMatrix3D.ScaleWith(modelingMatrix.scale());
                        using var orientationMatrix = eyeToWorldTranslatedMatrix * scaleMatrix;
                        m.setModelingMatrix(entityId, orientationMatrix, true);
                        _hiddenEntityList.Remove(handleId);
                    }
                    else
                    {
                        Hide(handleId);
                        if (!_hiddenEntityList.Contains(handleId))
                        {
                            _hiddenEntityList.Add(handleId);
                        }
                    }
                });
                return OdTvResult.tvOk;
            });
        }
        public void UpdateVisibility(OdTvGsViewId view, params ulong[] entityHandles)
        {
            SetValue(m =>
            {
                entityHandles.ForEach(handleId =>
                {
                    var isCadRotated = view.IsCADRotated();
                    if (isCadRotated)
                    {
                        Show(handleId);
                        _hiddenEntityList.Remove(handleId);
                    }
                    else
                    {
                        Hide(handleId);
                        if (!_hiddenEntityList.Contains(handleId))
                        {
                            _hiddenEntityList.Add(handleId);
                        }
                    }
                });
                return OdTvResult.tvOk;
            });
        }

        internal void RemoveModel(OdTvDatabaseId dbId)
        {
            using var db = dbId.openObject(OdTv_OpenMode.kForWrite);
            db.removeModel(_tvModelId);
        }
        internal void RemoveModel(IHclTooling hclTooling)
        {
            using var tvGsViewId = hclTooling.GetViewId();
            using var tvGsViewObj = tvGsViewId.openObject(OdTv_OpenMode.kForWrite);
            tvGsViewObj.eraseModel(_tvModelId);
            using var db = hclTooling.TvDatabaseId.openObject(OdTv_OpenMode.kForWrite);
            db.removeModel(_tvModelId);
        }

        #region Transformations
        public void UpdateLocation(CadPoint3D location)
        {
            SetValue(m =>
            {
                using var modelingMatrix = GetValue(model => model.getModelingMatrix());
                using var translatedModelingMatrix = modelingMatrix.setTranslation(location.AsVector());
                return m.setModelingMatrix(translatedModelingMatrix, true);
            });
        }
        public void UpdateScaleOfEntity(double scaleFactor)
        {
            SetValue(m =>
            {
                using var modelPtr = _tvModelId.openObject(OdTv_OpenMode.kForWrite);
                using var entityIterator = modelPtr.getEntitiesIterator();
                //int counter = 0;
                for (; !entityIterator.done(); entityIterator.step())
                {
                    using var entId = entityIterator.getEntity();
                    using var entityObj = entId.openObject(OdTv_OpenMode.kForWrite);
                    using var modelingMatrix = entityObj.getModelingMatrix();
                    using var scalingMatrix = CadMatrix3D.ScaleWith(scaleFactor);
                    using var scaledModelingMatrix = modelingMatrix.preMultBy(scalingMatrix);
                    entityObj.setModelingMatrix(scaledModelingMatrix);
                    //Debug.WriteLine($"Update Point Scale : {counter++}");
                }
                return OdTvResult.tvOk;
            });
        }
        public void UpdateEntityOrientation(OdTvGsViewId view, params ulong[] entityIds)
        {
            SetValue(m =>
            {
                entityIds.ForEach(handleId =>
                {
                    var entityId = m.findEntity(handleId);
                    using var entityObj = entityId.openObject(OdTv_OpenMode.kForWrite);
                    using var modelingMatrix = entityObj.getModelingMatrix();
                    using var eyeToWorldMatrix = view.EyeToWorldMatrix();
                    using var originVector = CadVector3D.Default;
                    using var eyeToWorldTranslatedMatrix = eyeToWorldMatrix.SetTranslation(originVector);
                    using var scaleMatrix = CadMatrix3D.ScaleWith(modelingMatrix.scale());
                    using var orientationMatrix = eyeToWorldTranslatedMatrix * scaleMatrix;
                    entityObj.setModelingMatrix(orientationMatrix);
                });
                return OdTvResult.tvOk;
            });
        }
        public void UpdateScaleOfPoint(List<CadPoint3D> locationList, double scaleFactor)
        {
            SetValue(m =>
            {
                using var modelPtr = _tvModelId.openObject(OdTv_OpenMode.kForWrite);
                using var entityIterator = modelPtr.getEntitiesIterator();
                int index = 0;
                for (; !entityIterator.done(); entityIterator.step())
                {
                    using var entId = entityIterator.getEntity();
                    using var entityObj = entId.openObject(OdTv_OpenMode.kForWrite);
                    using var modelingMatrix = entityObj.getModelingMatrix();
                    using var scalingMatrix = CadMatrix3D.ScaleWithCenterPoint(scaleFactor, locationList[index++]);
                    using var scaledModelingMatrix = modelingMatrix.preMultBy(scalingMatrix);
                    entityObj.setModelingMatrix(scaledModelingMatrix);
                }
                return OdTvResult.tvOk;
            });
        }

        internal void UpdateModelTransformationDelta(OdTvGsViewId view, List<CadPoint3D> locationList, double scaleFactor)
        {
            SetValue(m =>
            {
                using var modelPtr = _tvModelId.openObject(OdTv_OpenMode.kForWrite);
                using var entityIterator = modelPtr.getEntitiesIterator();
                using var eyeToWorldMatrix = view.EyeToWorldMatrix();
                int index = 0;
                for (; !entityIterator.done(); entityIterator.step())
                {
                    using var entId = entityIterator.getEntity();
                    var circleCenter = locationList[index++];

                    using var circleScaledMatrix = CadMatrix3D.ScaleWithCenterPoint(scaleFactor, circleCenter);
                    using var eyeToWorldMatrixScaled = circleScaledMatrix.PostMultiplyWith(eyeToWorldMatrix);
                    using var circleCenterRotated = CadPoint3D.With(circleCenter); 
                    circleCenterRotated.TransformWith(eyeToWorldMatrixScaled); 
                    using var deltaRotated = circleCenter - circleCenterRotated;
                    using var eyeToWorldTranslationPart = CadVector3D.With(eyeToWorldMatrixScaled.Translation());
                    using var deltaWcsLocation = eyeToWorldTranslationPart + deltaRotated;
                    using var resultingMatrixBackToCenter = eyeToWorldMatrixScaled.SetTranslation(deltaWcsLocation);
                    m.setModelingMatrix(entId, resultingMatrixBackToCenter, true);
                }
                return OdTvResult.tvOk;
            });
        }
        internal void UpdateModelTransformation(OdTvGsViewId view, List<CadPoint3D> locationList, double scaleFactor, ViewInverseMatrix viewInverseMatrix)
        {
            SetValue(m =>
            {
                using var modelPtr = _tvModelId.openObject(OdTv_OpenMode.kForWrite);
                using var entityIterator = modelPtr.getEntitiesIterator();
                using var eyeToWorldMatrix = view.EyeToWorldMatrix();

                for (int index = 0; !entityIterator.done(); entityIterator.step())
                {
                    using var entId = entityIterator.getEntity();
                    var circleCenter = locationList[index++];

                    using var scaleAtPointCenterMatrix = CadMatrix3D.ScaleWithCenterPoint(scaleFactor, circleCenter);
                    using var alingPointViewMatrix = CadMatrix3D.AlignCoordSys(
                               circleCenter, viewInverseMatrix.XAxis, viewInverseMatrix.YAxis, viewInverseMatrix.ZAxis, 
                               circleCenter, eyeToWorldMatrix.XAxis(), eyeToWorldMatrix.YAxis(), eyeToWorldMatrix.ZAxis());
                    using var modelingMatrix = scaleAtPointCenterMatrix * alingPointViewMatrix;
                    m.setModelingMatrix(entId, modelingMatrix, true);
                }
                return OdTvResult.tvOk;
            });
        }
        #endregion
    }
}
