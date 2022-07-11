﻿#pragma warning disable 618
namespace Core6
{
    using System;
    using NServiceBus;

    class Upgrade
    {

        void EnablingCriticalTime(EndpointConfiguration endpointConfiguration)
        {
  
        }
        void EnablingSla(EndpointConfiguration endpointConfiguration)
        {
            #region 6to1-enable-sla

            endpointConfiguration.EnableSLAPerformanceCounter(TimeSpan.FromMinutes(3));

            #endregion
        }


    }

}