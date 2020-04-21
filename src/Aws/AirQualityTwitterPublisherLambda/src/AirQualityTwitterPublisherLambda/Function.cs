using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
//[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.LambdaJsonSerializer))]

namespace AirQualityTwitterPublisherLambda
{
    public class Function
    {

        /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public void FunctionHandler(System.IO.Stream inputStream, ILambdaContext context)
        {
            System.IO.StreamReader reader = new System.IO.StreamReader(inputStream);
            var receivedMessage = reader.ReadToEnd();
            System.Console.WriteLine("This is where twitter API integration will happen");
            System.Console.WriteLine(receivedMessage);
        }
    }
}
