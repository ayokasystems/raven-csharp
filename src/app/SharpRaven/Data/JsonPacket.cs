﻿using System;
using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using SharpRaven.Serialization;

namespace SharpRaven.Data
{
    /// <summary>
    /// Represents the JSON packet that is transmitted to Sentry.
    /// </summary>
    public class JsonPacket
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JsonPacket"/> class.
        /// </summary>
        /// <param name="project">The project.</param>
        public JsonPacket(string project)
        {
            // Get assemblies.
            /*Modules = new List<Module>();
            foreach (System.Reflection.Module m in Utilities.SystemUtil.GetModules()) {
                Modules.Add(new Module() {
                    Name = m.ScopeName,
                    Version = m.ModuleVersionId.ToString()
                });
            }*/
            // The current hostname
            ServerName = Environment.MachineName;
            // Create timestamp
            TimeStamp = DateTime.UtcNow;
            // Default logger.
            Logger = "root";
            // Default error level.
            Level = ErrorLevel.Error;
            // Create a guid.
            EventID = Guid.NewGuid().ToString().Replace("-", String.Empty);
            // Project
            Project = "default";
            // Platform
            Platform = "csharp";

            // Get data from the HTTP request
            Request = SentryRequest.GetRequest();
            // Get the user data from the HTTP request
            if (Request != null)
                User = Request.GetUser();                

            Project = project;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="JsonPacket"/> class.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="e">The decimal.</param>
        public JsonPacket(string project, Exception e)
        {
            Message = e.Message;

            if (e.TargetSite != null)
            {
// ReSharper disable ConditionIsAlwaysTrueOrFalse => not for dynamic types.
                Culprit = String.Format("{0} in {1}",
                                        ((e.TargetSite.ReflectedType == null)
                                             ? "<dynamic type>"
                                             : e.TargetSite.ReflectedType.FullName),
                                        e.TargetSite.Name);
// ReSharper restore ConditionIsAlwaysTrueOrFalse
            }

            Project = project;
            ServerName = Environment.MachineName;
            Level = ErrorLevel.Error;

            Exceptions = new List<SentryException>();

            for (Exception currentException = e;
                 currentException != null;
                 currentException = currentException.InnerException)
            {
                SentryException sentryException = new SentryException(currentException);
                sentryException.Module = currentException.Source;
                sentryException.Type = currentException.GetType().Name;
                sentryException.Value = currentException.Message;
                Exceptions.Add(sentryException);
            }
        }


        /// <summary>
        /// An arbitrary mapping of additional metadata to store with the event.
        /// </summary>
        [JsonProperty(PropertyName = "extra", NullValueHandling = NullValueHandling.Ignore)]
        public object Extra { get; set; }

        /// <summary>
        /// A map or list of tags for this event.
        /// </summary>
        [JsonProperty(PropertyName = "tags", NullValueHandling = NullValueHandling.Ignore)]
        public IDictionary<string, string> Tags { get; set; }


        /// <summary>
        /// Hexadecimal string representing a uuid4 value.
        /// </summary>
        [JsonProperty(PropertyName = "event_id", NullValueHandling = NullValueHandling.Ignore)]
        public string EventID { get; set; }

        /// <summary>
        /// String value representing the project
        /// </summary>
        [JsonProperty(PropertyName = "project", NullValueHandling = NullValueHandling.Ignore)]
        public string Project { get; set; }

        /// <summary>
        /// Function call which was the primary perpetrator of this event.
        /// A map or list of tags for this event.
        /// </summary>
        [JsonProperty(PropertyName = "culprit", NullValueHandling = NullValueHandling.Ignore)]
        public string Culprit { get; set; }

        /// <summary>
        /// The record severity.
        /// Defaults to error.
        /// </summary>
        [JsonProperty(PropertyName = "level", NullValueHandling = NullValueHandling.Ignore, Required = Required.Always)]
        [JsonConverter(typeof(ErrorLevelConverter))]
        public ErrorLevel Level { get; set; }

        /// <summary>
        /// Indicates when the logging record was created (in the Sentry client).
        /// Defaults to DateTime.UtcNow()
        /// </summary>
        [JsonProperty(PropertyName = "timestamp", NullValueHandling = NullValueHandling.Ignore)]
        public DateTime TimeStamp { get; set; }

        /// <summary>
        /// The name of the logger which created the record.
        /// If missing, defaults to the string root.
        /// 
        /// Ex: "my.logger.name"
        /// </summary>
        [JsonProperty(PropertyName = "logger", NullValueHandling = NullValueHandling.Ignore)]
        public string Logger { get; set; }

        /// <summary>
        /// A string representing the platform the client is submitting from. 
        /// This will be used by the Sentry interface to customize various components in the interface.
        /// </summary>
        [JsonProperty(PropertyName = "platform", NullValueHandling = NullValueHandling.Ignore)]
        public string Platform { get; set; }

        /// <summary>
        /// User-readable representation of this event
        /// </summary>
        [JsonProperty(PropertyName = "message", NullValueHandling = NullValueHandling.Ignore)]
        public string Message { get; set; }

        /// <summary>
        /// Identifies the host client from which the event was recorded.
        /// </summary>
        [JsonProperty(PropertyName = "server_name", NullValueHandling = NullValueHandling.Ignore)]
        public string ServerName { get; set; }

        /// <summary>
        /// A list of relevant modules (libraries) and their versions.
        /// Automated to report all modules currently loaded in project.
        /// </summary>
        /// <value>
        /// The modules.
        /// </value>
        [JsonProperty(PropertyName = "modules", NullValueHandling = NullValueHandling.Ignore)]
        public List<Module> Modules { get; set; }

        /// <summary>
        /// Gets or sets the exceptions.
        /// </summary>
        /// <value>
        /// The exceptions.
        /// </value>
        [JsonProperty(PropertyName = "exception", NullValueHandling = NullValueHandling.Ignore)]
        public List<SentryException> Exceptions { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="SentryRequest"/> object, containing information about the HTTP request.
        /// </summary>
        /// <value>
        /// The <see cref="SentryRequest"/> object, containing information about the HTTP request.
        /// </value>
        [JsonProperty(PropertyName = "request", NullValueHandling = NullValueHandling.Ignore)]
        public SentryRequest Request { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="SentryUser"/> object, which describes the authenticated User for a request.
        /// </summary>
        /// <value>
        /// The <see cref="SentryUser"/> object, which describes the authenticated User for a request.
        /// </value>
        [JsonProperty(PropertyName = "user", NullValueHandling = NullValueHandling.Ignore)]
        public SentryUser User { get; set; }
        
        /// <summary>
        /// Converts the <see cref="JsonPacket"/> into a JSON string.
        /// </summary>
        /// <returns>
        /// The <see cref="JsonPacket"/> as a JSON string.
        /// </returns>
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);

            //return @"{""project"": ""SharpRaven"",""event_id"": ""fc6d8c0c43fc4630ad850ee518f1b9d0"",""culprit"": ""my.module.function_name"",""timestamp"": ""2012-11-11T17:41:36"",""message"": ""SyntaxError: Wattttt!"",""sentry.interfaces.Exception"": {""type"": ""SyntaxError"",""value"": ""Wattttt!"",""module"": ""__builtins__""}""}";
        }
    }
}