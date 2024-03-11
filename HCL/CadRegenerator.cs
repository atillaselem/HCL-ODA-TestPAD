using HCL_ODA_TestPAD.Mvvm.Events;
using HCL_ODA_TestPAD.Settings;
using System;
using ODA.Visualize.TV_Visualize;

namespace HCL_ODA_TestPAD.HCL
{
    public class CadRegenerator
    {
        private readonly IServiceFactory _serviceFactory;

        /// <summary>
        /// Auto-Regeneration parameters
        /// </summary>
        //private const double _regenThreshold = 0.01;
        private double _lastRegenCoeff;


        public CadRegenerator(IServiceFactory serviceFactory)
        {
            _serviceFactory = serviceFactory;
            serviceFactory.EventSrv.GetEvent<ProgressMaxChangedEvent>().Publish(100);
        }

        private (bool, double) CheckAutoRegeneration(OdTvGsDevice dev)
        {
            var canRegenerate = false;
            var currentCoeff = 0d;
            currentCoeff = GetRegenCoefficient(dev);
            if (currentCoeff >= _serviceFactory.AppSettings.RegenThreshold && currentCoeff == _lastRegenCoeff)
            {
                canRegenerate = false;
            }

            // regen coeff  tbeyond the threshold, regeneration is mandatory
            //else if (currentCoeff >= _serviceFactory.AppSettings.RegenThreshold) // HCL Implementation
            else if (currentCoeff >= _serviceFactory.AppSettings.RegenThreshold && currentCoeff > _lastRegenCoeff) //TestPAD Fix Perfromance! 
            {
                canRegenerate = true;
            }
            return (canRegenerate, currentCoeff);
        }
        private double GetRegenCoefficient(OdTvGsDevice dev)
        {
            dev.getOption(OdTvGsDevice_Options.kRegenCoef, out double result);
            return result;
        }

        public OdTvGsDevice TryAutoRegeneration(OdTvGsDevice dev)
        {
            if (_serviceFactory.AppSettings.AutoRegeneration)
            {
                var (canRegenerate, currentCoeff) = CheckAutoRegeneration(dev);

                if (canRegenerate)
                {
                    dev.regen(_serviceFactory.AppSettings.RegenMode);

                    _lastRegenCoeff = currentCoeff;

                }

                _serviceFactory.EventSrv.GetEvent<ProgressStepChangedEvent>().Publish(new ProgressStepChangedEventArg()
                {
                    CurrentDeviceCoefficient = currentCoeff,
                    RegenThreshold = _serviceFactory.AppSettings.RegenThreshold,
                    LastDeviceCoefficientAfterRegen = _lastRegenCoeff,
                    CurrentProgressStep = canRegenerate ? 5 : 0
                });
            }
            return dev;
        }
    }

    public static class CadRegeneratorExtensions
    {
        public static OdTvGsDevice TryAutoRegeneration(this OdTvGsDevice dev, Func<CadRegenerator> cadGenFactory)
            => cadGenFactory().TryAutoRegeneration(dev);
    }
}
