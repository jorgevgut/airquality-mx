using Amazon.CDK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AirQualityCdk
{
    sealed class Program
    {
        public static void Main(string[] args)
        {
            var app = new App();
            new AirQualityCdkStack(app, "AirQualityCdkStack");
            app.Synth();
        }
    }
}
