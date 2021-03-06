﻿// <copyright file="ChangeTicketStatusPayload.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Common.Models
{
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the data payload of Action.Submit to change the status of a ticket.
    /// </summary>
    public class ChangeTicketStatusPayload
    {
        /// <summary>
        /// Action that change a ticket to pending.
        /// </summary>
        public const string PendingAction = "Pending";

        /// <summary>
        /// Action that resolve a ticket.
        /// </summary>
        public const string ResolveAction = "Resolve";

        /// <summary>
        /// Action that assigns a ticket to the person that performed the action.
        /// </summary>
        public const string AssignToSelfAction = "AssignToSelf";

        /// <summary>
        /// Action that assigns a ticket to the other persion in expert channel.
        /// </summary>
        public const string AssignToOthersAction = "AssignToOthers";

        /// <summary>
        /// Gets or sets the ticket id.
        /// </summary>
        [JsonProperty("ticketId")]
        public string TicketId { get; set; }

        /// <summary>
        /// Gets or sets the action to perform on the ticket.
        /// </summary>
        [JsonProperty("action")]
        public string Action { get; set; }

        /// <summary>
        /// Gets or sets the comment for pending.
        /// </summary>
        [JsonProperty("pendingComment")]
        public string PendingComment { get; set; }

        /// <summary>
        /// Gets or sets the comment for resolve.
        /// </summary>
        [JsonProperty("resolveComment")]
        public string ResolveComment { get; set; }

        /// <summary>
        /// Gets or sets the info for other assignee name:ID.
        /// </summary>
        [JsonProperty("otherAssigneeInfo")]
        public string OtherAssigneeInfo { get; set; }
    }
}