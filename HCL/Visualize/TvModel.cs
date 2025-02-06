using HCL_ODA_TestPAD.Functional.Extensions;
using HCL_ODA_TestPAD.HCL.CAD.Math.API;
using ODA.Visualize.TV_Visualize;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using HCL_ODA_TestPAD.HCL.Visualize.Extensions;
using HCL_ODA_TestPAD.ViewModels.Base;
using ODA.Kernel.TD_RootIntegrated;
using System.Security.Cryptography;

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

        #region Transformation Not Used

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


        #endregion

        internal void RemoveModel(IHclTooling hclTooling)
        {
            using var tvGsViewId = hclTooling.GetViewId();
            using var tvGsViewObj = tvGsViewId.openObject(OdTv_OpenMode.kForWrite);
            tvGsViewObj.eraseModel(_tvModelId);
            using var db = hclTooling.TvDatabaseId.openObject(OdTv_OpenMode.kForWrite);
            db.removeModel(_tvModelId);
        }

        #region Transformations Active
        public void UpdateLocation(CadPoint3D location)
        {
            SetValue(m =>
            {
                using var modelingMatrix = GetValue(model => model.getModelingMatrix());
                using var translatedModelingMatrix = modelingMatrix.setTranslation(location.AsVector());
                return m.setModelingMatrix(translatedModelingMatrix, true);
            });
        }
        public void UpdateLocationDelta(CadVector3D deltaLocation)
        {
            SetValue(m =>
            {
                using var modelingMatrix = GetValue(model => model.getModelingMatrix());
                using var deltaMatrix = CadMatrix3D.Translation(deltaLocation);
                using var translatedModelingMatrix = modelingMatrix.preMultBy(deltaMatrix);
                return m.setModelingMatrix(translatedModelingMatrix, true);
            });
        }
        public void UpdateModelScale(double scaleFactor, CadPoint3D location)
        {
            SetValue(m =>
            {
                using var modelingMatrix = m.getModelingMatrix();
                using var scalingMatrix = CadMatrix3D.ScaleWithCenterPoint(scaleFactor, location);
                using var scaledModelingMatrix = modelingMatrix.preMultBy(scalingMatrix);
                return m.setModelingMatrix(scaledModelingMatrix, true);
            });
        }
        public void UpdateScaleOfEntity(double scaleFactor)
        {
            SetValue(m =>
            {
                using var modelPtr = _tvModelId.openObject(OdTv_OpenMode.kForWrite);
                using var entityIterator = modelPtr.getEntitiesIterator();
                for (; !entityIterator.done(); entityIterator.step())
                {
                    using var entId = entityIterator.getEntity();
                    using var entityObj = entId.openObject(OdTv_OpenMode.kForWrite);
                    using var modelingMatrix = entityObj.getModelingMatrix();
                    using var scalingMatrix = CadMatrix3D.ScaleWith(scaleFactor);
                    using var scaledModelingMatrix = modelingMatrix.preMultBy(scalingMatrix);
                    entityObj.setModelingMatrix(scaledModelingMatrix);
                }
                return OdTvResult.tvOk;
            });
        }
        public void UpdateScaleOfEntityAtLocation(double scaleFactor, CadPoint3D location)
        {
            SetValue(m =>
            {
                using var modelPtr = _tvModelId.openObject(OdTv_OpenMode.kForWrite);
                using var entityIterator = modelPtr.getEntitiesIterator();
                for (; !entityIterator.done(); entityIterator.step())
                {
                    using var entId = entityIterator.getEntity();
                    using var entityObj = entId.openObject(OdTv_OpenMode.kForWrite);
                    using var modelingMatrix = entityObj.getModelingMatrix();
                    using var scalingMatrix = CadMatrix3D.ScaleWithCenterPoint(scaleFactor, location);
                    using var scaledModelingMatrix = modelingMatrix.preMultBy(scalingMatrix);
                    entityObj.setModelingMatrix(scaledModelingMatrix);
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
                    using var entityId = m.findEntity(handleId);
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

        public void UpdateEntityOrientationEx(OdTvGsViewId view, params ulong[] entityIds)
        {
            SetValue(m =>
            {
                entityIds.ForEach(handleId =>
                {
                    using var entityId = m.findEntity(handleId);
                    using var entityObj = entityId.openObject(OdTv_OpenMode.kForWrite);
                    using var modelingMatrix = entityObj.getModelingMatrix();
                    using var eyeToWorldMatrix = view.EyeToWorldMatrix();
                    using var originVector = CadVector3D.Default;
                    using var eyeToWorldTranslatedMatrix = eyeToWorldMatrix.SetTranslation(originVector);
                    using var scaleMatrix = CadMatrix3D.ScaleWith(modelingMatrix.scale());
                    using var orientationMatrix = eyeToWorldTranslatedMatrix * scaleMatrix;
                    m.setModelingMatrix(entityId, orientationMatrix, true);
                });
                return OdTvResult.tvOk;
            });
        }
        public void UpdateEntityOrientationAtLocation(OdTvGsViewId view, CadPoint3D location, params ulong[] entityIds)
        {
            SetValue(m =>
            {
                entityIds.ForEach(handleId =>
                {
                    using var entityId = m.findEntity(handleId);
                    using var entityObj = entityId.openObject(OdTv_OpenMode.kForWrite);
                    using var modelingMatrix = entityObj.getModelingMatrix();
                    using var eyeToWorldMatrix = view.EyeToWorldMatrix();
                    using var originVector = CadVector3D.Default;
                    using var eyeToWorldTranslatedReset = eyeToWorldMatrix.SetTranslation(originVector);
                    using var scaleMatrix = CadMatrix3D.ScaleWithCenterPoint(modelingMatrix.scale(), location);
                    using var scaleMatrixTranslateReset = CadMatrix3D.ScaleWithCenterPoint(modelingMatrix.scale(), location);
                    using var orientationMatrix = eyeToWorldTranslatedReset * scaleMatrixTranslateReset;
                    entityObj.setModelingMatrix(orientationMatrix);
                });
                return OdTvResult.tvOk;
            });
        }
        internal void UpdateModelViewTransformations(OdTvGsViewId view, List<CadPoint3D> locationList, params ulong[] entityIds)
        {
            SetValue(model =>
            {
                using var entityIterator = model.getEntitiesIterator();
                using var eyeToWorldMatrix = view.EyeToWorldMatrix().SetTranslation(CadVector3D.Origin);

                for (var index = 0; index < entityIds.Length; index++)
                {
                    using var entityId = model.findEntity(entityIds[index]);
                    var entityLocation = locationList[index];
                    using var alignCoordinateSysMatrix = CadMatrix3D.AlignCoordSys(
                        entityLocation, CadVector3D.XAxis, CadVector3D.YAxis, CadVector3D.ZAxis,
                        entityLocation, eyeToWorldMatrix.XAxis(), eyeToWorldMatrix.YAxis(), eyeToWorldMatrix.ZAxis());
                    model.setModelingMatrix(entityId, alignCoordinateSysMatrix, true);
                }
                return OdTvResult.tvOk;
            });
        }
        internal void UpdateCrossViewTransformations(OdTvGsViewId view, List<CadPoint3D> locationList, params ulong[] entityHandles)
        {
            SetValue(model =>
            {
                using var eyeVector = view.Direction(); //(position - target).normalize();
                using var xAxis = view.CsXVector();
                using var yAxis = view.UpVector();

                //Get Circle (index = 0) Normal
                using var circleEntityId = model.findEntity(entityHandles[0]);
                using var circleEntityObj = circleEntityId.openObject();
                using var circleGeoIterator = circleEntityObj.getGeometryDataIterator();
                using var circleGeometryData = circleGeoIterator.getGeometryData();
                var circleGeometryType = circleGeometryData.getType();

                if (circleGeometryType != OdTv_OdTvGeometryDataType.kPolyline)
                {
                    return OdTvResult.tvEmptySubEntity;
                }
                using var circlePolyline = circleGeometryData.openAsPolyline();
                using var circlePointArray = new OdGePoint3dVector();
                circlePolyline.getPoints(circlePointArray);
                var pointCount = circlePointArray.Count;
                using var plane = new OdGePlane(circlePointArray[0], circlePointArray[pointCount/2], circlePointArray[pointCount/4]);
                using var circleNormal = plane.normal();

                //Project Cross Line Vectors on Circle Surface (index = 1 && 2)
                for (int index = 1; index < entityHandles.Length; index++)
                {
                    using var entityId = model.findEntity(entityHandles[index]);
                    using var entityObj = entityId.openObject(OdTv_OpenMode.kForWrite);
                    OdGeVector3d refCamProj = null;
                    if (index == 1)
                    {
                        refCamProj = yAxis.Project(circleNormal, eyeVector);
                    }
                    else
                    {
                        refCamProj = xAxis.Project(circleNormal, eyeVector);
                    }
                    using var geoIterator = entityObj.getGeometryDataIterator();
                    using var geometryItem = geoIterator.getGeometryData();
                    var geometryType = geometryItem.getType();
                    if (geometryType != OdTv_OdTvGeometryDataType.kPolyline)
                    {
                        return OdTvResult.tvEmptySubEntity;
                    }
                    using var polyline = geometryItem.openAsPolyline();
                    using var pointArray = new OdGePoint3dVector();
                    polyline.getPoints(pointArray);
                    using var crossLineVector = pointArray[1] - pointArray[0];
                    double angle = crossLineVector.angleTo(refCamProj, circleNormal);
                    using var modelingMatrix = entityObj.getModelingMatrix();
                    var entityLocation = locationList[index];
                    using var rotationMatrix = CadMatrix3D.RotationAtCenter(angle, circleNormal, entityLocation);
                    using var rotatedModelingMatrix = modelingMatrix.preMultBy(rotationMatrix);
                    model.setModelingMatrix(entityId, rotatedModelingMatrix, true);
                }
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
            SetValue(model =>
            {
                using var entityIterator = model.getEntitiesIterator();
                using var eyeToWorldMatrix = view.EyeToWorldMatrix();

                for (int index = 0; !entityIterator.done(); entityIterator.step())
                {
                    using var entId = entityIterator.getEntity();
                    var circleCenter = locationList[index++];

                    using var scaleAtPointCenterMatrix = CadMatrix3D.ScaleWithCenterPoint(scaleFactor, circleCenter);
                    using var alinePointViewMatrix = CadMatrix3D.AlignCoordSys(
                               circleCenter, viewInverseMatrix.XAxis, viewInverseMatrix.YAxis, viewInverseMatrix.ZAxis, 
                               circleCenter, eyeToWorldMatrix.XAxis(), eyeToWorldMatrix.YAxis(), eyeToWorldMatrix.ZAxis());
                    using var modelingMatrix = scaleAtPointCenterMatrix * alinePointViewMatrix;
                    model.setModelingMatrix(entId, modelingMatrix, true);
                }
                return OdTvResult.tvOk;
            });
        }

        internal void ToogleVisibility(bool showText)
        {
            SetValue(model =>
            {
                using var entIterator = model.getEntitiesIterator();
                for (; !entIterator.done(); entIterator.step())
                {
                    using var entId = entIterator.getEntity();
                    using var entityOpen = entId.openObject(OdTv_OpenMode.kForRead);
                    using var geoIterator = entityOpen.getGeometryDataIterator();
                    for (; !geoIterator.done(); geoIterator.step())
                    {
                        using var geometryData = geoIterator.getGeometryData();
                        using var geometryDataOpen = geometryData.openObject();
                        var type = geometryDataOpen.getType();
                        if (type == OdTv_OdTvGeometryDataType.kText)
                        {
                            if(showText)
                            {
                                model.unHide(geometryData);
                            }
                            else
                            {
                                model.hide(geometryData);
                            }
                        }
                    }
                }
                return OdTvResult.tvOk;
            });
        }
        internal void ToogleVisibility(OdTvEntityId odTvEntityId, bool showText)
        {
            SetValue(model =>
            {
                if (showText)
                {
                    model.unHide(odTvEntityId);
                }
                else
                {
                    model.hide(odTvEntityId);
                }
                return OdTvResult.tvOk;
            });
        }
        #endregion

        public T GetImplementation<T>() where T : class
        {
            return _tvModelId as T;
        }

        public T OpenAs<T>() where T : class
        {
            return _tvModelId.openObject() as T;
        }
    }
}

