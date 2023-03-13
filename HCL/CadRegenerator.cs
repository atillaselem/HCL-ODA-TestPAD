using HCL_ODA_TestPAD.Mvvm.Events;
using HCL_ODA_TestPAD.Settings;
using Prism.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teigha.Visualize;

namespace HCL_ODA_TestPAD.HCL
{
    public class CadRegenerator
    {
        /// <summary>
        /// Auto-Regeneration parameters
        /// </summary>
        //private const double _regenThreshold = 0.01;
        private double _lastRegenCoeff;
        private readonly Func<IAppSettings> _appSettings;
        private readonly Func<IEventAggregator> _eventAggregator;

        public CadRegenerator(Func<IAppSettings> appSettings, Func<IEventAggregator> eventAggregator)
        {
            _appSettings = appSettings;
            _eventAggregator = eventAggregator;
            _eventAggregator().GetEvent<ProgressMaxChangedEvent>().Publish(100);
        }

        private (bool, double) CheckAutoRegeneration(OdTvGsDevice dev)
        {
            var canRegenerate = false;
            var currentCoeff = 0d;
            currentCoeff = GetRegenCoefficient(dev);
            if (currentCoeff >= _appSettings().RegenThreshold && currentCoeff == _lastRegenCoeff)
            {
                canRegenerate = false;
            }

            // regen coeff beyond the threshold, regeneration is mandatory
            //else if (currentCoeff >= _appSettings().RegenThreshold) // HCL Implementation
            else if (currentCoeff >= _appSettings().RegenThreshold && currentCoeff > _lastRegenCoeff) //TestPAD Fix Perfromance! 
            {
                canRegenerate = true;
            }
            return (canRegenerate, currentCoeff);
        }
        private double GetRegenCoefficient(OdTvGsDevice dev)
        {
            dev.getOption(OdTvGsDevice.Options.kRegenCoef, out double result);
            return result;
        }

        public OdTvGsDevice TryAutoRegeneration(OdTvGsDevice dev)
        {
            if (_appSettings().AutoRegeneration)
            {
                var (canRegenerate, currentCoeff) = CheckAutoRegeneration(dev);

                if (canRegenerate)
                {
                    dev.regen(_appSettings().RegenMode);

                    _lastRegenCoeff = currentCoeff;

                }

                _eventAggregator().GetEvent<ProgressStepChangedEvent>().Publish(new ProgressStepChangedEventArg()
                {
                    CurrentDeviceCoefficient = currentCoeff,
                    RegenThreshold = _appSettings().RegenThreshold,
                    LastDeviceCoefficientAfterRegen = _lastRegenCoeff,
                    CurrentProgressStep = canRegenerate ? 5 : 0
                }); ;
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
