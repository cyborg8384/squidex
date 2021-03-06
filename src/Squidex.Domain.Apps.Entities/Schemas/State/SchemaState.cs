﻿// ==========================================================================
//  Squidex Headless CMS
// ==========================================================================
//  Copyright (c) Squidex UG (haftungsbeschränkt)
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using Newtonsoft.Json;
using Squidex.Domain.Apps.Core;
using Squidex.Domain.Apps.Core.Schemas;
using Squidex.Domain.Apps.Events;
using Squidex.Domain.Apps.Events.Schemas;
using Squidex.Infrastructure;
using Squidex.Infrastructure.Dispatching;
using Squidex.Infrastructure.EventSourcing;
using Squidex.Infrastructure.Reflection;
using Squidex.Infrastructure.States;

namespace Squidex.Domain.Apps.Entities.Schemas.State
{
    [CollectionName("Schemas")]
    public class SchemaState : DomainObjectState<SchemaState>, ISchemaEntity
    {
        [JsonProperty]
        public NamedId<Guid> AppId { get; set; }

        [JsonProperty]
        public string Name { get; set; }

        [JsonProperty]
        public string Category { get; set; }

        [JsonProperty]
        public int TotalFields { get; set; } = 0;

        [JsonProperty]
        public bool IsDeleted { get; set; }

        [JsonProperty]
        public bool IsSingleton { get; set; }

        [JsonProperty]
        public string ScriptQuery { get; set; }

        [JsonProperty]
        public string ScriptCreate { get; set; }

        [JsonProperty]
        public string ScriptUpdate { get; set; }

        [JsonProperty]
        public string ScriptDelete { get; set; }

        [JsonProperty]
        public string ScriptChange { get; set; }

        [JsonProperty]
        public Schema SchemaDef { get; set; }

        [JsonIgnore]
        public bool IsPublished
        {
            get { return SchemaDef.IsPublished; }
        }

        protected void On(SchemaCreated @event, FieldRegistry registry)
        {
            Name = @event.Name;

            IsSingleton = @event.Singleton;

            var schema = new Schema(@event.Name);

            if (@event.Properties != null)
            {
                schema = schema.Update(@event.Properties);
            }

            if (@event.Publish)
            {
                schema = schema.Publish();
            }

            if (@event.Fields != null)
            {
                foreach (var eventField in @event.Fields)
                {
                    TotalFields++;

                    var partitioning = Partitioning.FromString(eventField.Partitioning);

                    var field = registry.CreateRootField(TotalFields, eventField.Name, partitioning, eventField.Properties);

                    if (field is ArrayField arrayField && eventField.Nested?.Count > 0)
                    {
                        foreach (var nestedEventField in eventField.Nested)
                        {
                            TotalFields++;

                            var nestedField = registry.CreateNestedField(TotalFields, nestedEventField.Name, nestedEventField.Properties);

                            if (nestedEventField.IsHidden)
                            {
                                nestedField = nestedField.Hide();
                            }

                            if (nestedEventField.IsDisabled)
                            {
                                nestedField = nestedField.Disable();
                            }

                            arrayField = arrayField.AddField(nestedField);
                        }

                        field = arrayField;
                    }

                    if (eventField.IsHidden)
                    {
                        field = field.Hide();
                    }

                    if (eventField.IsDisabled)
                    {
                        field = field.Disable();
                    }

                    if (eventField.IsLocked)
                    {
                        field = field.Lock();
                    }

                    schema = schema.AddField(field);
                }
            }

            SchemaDef = schema;

            AppId = @event.AppId;
        }

        protected void On(FieldAdded @event, FieldRegistry registry)
        {
            if (@event.ParentFieldId != null)
            {
                var field = registry.CreateNestedField(@event.FieldId.Id, @event.Name, @event.Properties);

                SchemaDef = SchemaDef.UpdateField(@event.ParentFieldId.Id, x => ((ArrayField)x).AddField(field));
            }
            else
            {
                var partitioning = Partitioning.FromString(@event.Partitioning);

                var field = registry.CreateRootField(@event.FieldId.Id, @event.Name, partitioning, @event.Properties);

                SchemaDef = SchemaDef.DeleteField(@event.FieldId.Id);
                SchemaDef = SchemaDef.AddField(field);
            }

            TotalFields++;
        }

        protected void On(SchemaCategoryChanged @event, FieldRegistry registry)
        {
            Category = @event.Name;
        }

        protected void On(SchemaPublished @event, FieldRegistry registry)
        {
            SchemaDef = SchemaDef.Publish();
        }

        protected void On(SchemaUnpublished @event, FieldRegistry registry)
        {
            SchemaDef = SchemaDef.Unpublish();
        }

        protected void On(SchemaUpdated @event, FieldRegistry registry)
        {
            SchemaDef = SchemaDef.Update(@event.Properties);
        }

        protected void On(SchemaFieldsReordered @event, FieldRegistry registry)
        {
            SchemaDef = SchemaDef.ReorderFields(@event.FieldIds, @event.ParentFieldId?.Id);
        }

        protected void On(FieldUpdated @event, FieldRegistry registry)
        {
            SchemaDef = SchemaDef.UpdateField(@event.FieldId.Id, @event.Properties, @event.ParentFieldId?.Id);
        }

        protected void On(FieldLocked @event, FieldRegistry registry)
        {
            SchemaDef = SchemaDef.LockField(@event.FieldId.Id, @event.ParentFieldId?.Id);
        }

        protected void On(FieldDisabled @event, FieldRegistry registry)
        {
            SchemaDef = SchemaDef.DisableField(@event.FieldId.Id, @event.ParentFieldId?.Id);
        }

        protected void On(FieldEnabled @event, FieldRegistry registry)
        {
            SchemaDef = SchemaDef.EnableField(@event.FieldId.Id, @event.ParentFieldId?.Id);
        }

        protected void On(FieldHidden @event, FieldRegistry registry)
        {
            SchemaDef = SchemaDef.HideField(@event.FieldId.Id, @event.ParentFieldId?.Id);
        }

        protected void On(FieldShown @event, FieldRegistry registry)
        {
            SchemaDef = SchemaDef.ShowField(@event.FieldId.Id, @event.ParentFieldId?.Id);
        }

        protected void On(FieldDeleted @event, FieldRegistry registry)
        {
            SchemaDef = SchemaDef.DeleteField(@event.FieldId.Id, @event.ParentFieldId?.Id);
        }

        protected void On(SchemaDeleted @event, FieldRegistry registry)
        {
            IsDeleted = true;
        }

        protected void On(ScriptsConfigured @event, FieldRegistry registry)
        {
            SimpleMapper.Map(@event, this);
        }

        public SchemaState Apply(Envelope<IEvent> @event, FieldRegistry registry)
        {
            var payload = (SquidexEvent)@event.Payload;

            return Clone().Update(payload, @event.Headers, r => r.DispatchAction(payload, registry));
        }
    }
}
