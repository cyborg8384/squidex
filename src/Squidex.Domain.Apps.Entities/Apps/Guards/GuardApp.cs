﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Threading.Tasks;
using Squidex.Domain.Apps.Core.Apps;
using Squidex.Domain.Apps.Entities.Apps.Commands;
using Squidex.Domain.Apps.Entities.Apps.Services;
using Squidex.Infrastructure;

namespace Squidex.Domain.Apps.Entities.Apps.Guards
{
    public static class GuardApp
    {
        public static Task CanCreate(CreateApp command, IAppProvider appProvider)
        {
            Guard.NotNull(command, nameof(command));

            return Validate.It(() => "Cannot create app.", async error =>
            {
                if (!command.Name.IsSlug())
                {
                    error(new ValidationError("Name must be a valid slug (lowercase characters, numbers and dashes).", nameof(command.Name)));
                }
                else if (await appProvider.GetAppAsync(command.Name) != null)
                {
                    error(new ValidationError($"An app with the same name already exists.", nameof(command.Name)));
                }
            });
        }

        public static void CanChangePlan(ChangePlan command, AppPlan plan, IAppPlansProvider appPlans)
        {
            Guard.NotNull(command, nameof(command));

            Validate.It(() => "Cannot change plan.", error =>
            {
                if (string.IsNullOrWhiteSpace(command.PlanId))
                {
                    error(new ValidationError("Plan id is required.", nameof(command.PlanId)));
                }
                else
                {
                    if (appPlans.GetPlan(command.PlanId) == null)
                    {
                        error(new ValidationError("A plan with this id does not exist.", nameof(command.PlanId)));
                    }

                    if (!string.IsNullOrWhiteSpace(command.PlanId) && plan != null && !plan.Owner.Equals(command.Actor))
                    {
                        error(new ValidationError("Plan can only changed from the user who configured the plan initially."));
                    }

                    if (string.Equals(command.PlanId, plan?.PlanId, StringComparison.OrdinalIgnoreCase))
                    {
                        error(new ValidationError("App has already this plan."));
                    }
                }
            });
        }
    }
}
