// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Jint;
using Microsoft.Extensions.Logging;

namespace Notifo.Domain.Integrations
{
    public sealed class ConditionEvaluator
    {
        private readonly ILogger log;

        public ConditionEvaluator(ILogger log)
        {
            this.log = log;
        }

        public bool Evaluate(string? script, IIntegrationTarget target)
        {
            if (string.IsNullOrEmpty(script))
            {
                return true;
            }

            var properties = target.Properties;

            if (properties == null)
            {
                return true;
            }

            try
            {
                var engine = new Engine();

                foreach (var (key, value) in properties)
                {
                    engine.SetValue(key, value);
                }

                var evaluation = engine.Evaluate(ConditionParser.Parse(script));

                return evaluation.IsBoolean() && evaluation.AsBoolean();
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Failed to evaluate script {script}.", script);
                return false;
            }
        }
    }
}
