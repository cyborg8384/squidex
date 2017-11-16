﻿// ==========================================================================
//  AppStateGrainState_Rules.cs
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex Group
//  All rights reserved.
// ==========================================================================

using System;
using Squidex.Domain.Apps.Events.Rules;
using Squidex.Domain.Apps.Events.Rules.Utils;
using Squidex.Infrastructure.CQRS.Events;

namespace Squidex.Domain.Apps.Read.State.Orleans.Grains.Implementations
{
    public sealed partial class AppStateGrainState
    {
        public void On(RuleCreated @event, EnvelopeHeaders headers)
        {
            Rules[@event.RuleId] = EntityMapper.Create<JsonRuleEntity>(@event, headers, r =>
            {
                r.RuleDef = RuleEventDispatcher.Create(@event);
            });
        }

        public void On(RuleUpdated @event, EnvelopeHeaders headers)
        {
            UpdateRule(@event, headers, r =>
            {
                r.RuleDef = r.RuleDef.Apply(@event);
            });
        }

        public void On(RuleEnabled @event, EnvelopeHeaders headers)
        {
            UpdateRule(@event, headers, r =>
            {
                r.RuleDef = r.RuleDef.Apply(@event);
            });
        }

        public void On(RuleDisabled @event, EnvelopeHeaders headers)
        {
            UpdateRule(@event, headers, r =>
            {
                r.RuleDef = r.RuleDef.Apply(@event);
            });
        }

        public void On(RuleDeleted @event, EnvelopeHeaders headers)
        {
            Rules.Remove(@event.RuleId);
        }

        private void UpdateRule(RuleEvent @event, EnvelopeHeaders headers, Action<JsonRuleEntity> updater = null)
        {
            Rules[@event.RuleId] = Rules[@event.RuleId].Clone().Update(@event, headers, updater);
        }
    }
}
