﻿// <copyright file="SmeTicketCard.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.FAQPlusPlus.Cards
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using AdaptiveCards;
    using Microsoft.Bot.Schema;
    using Microsoft.Teams.Apps.FAQPlusPlus.Common.Models;
    using Microsoft.Teams.Apps.FAQPlusPlus.Properties;

    /// <summary>
    /// Represents an SME ticket used for both in place card update activity within SME channel
    /// when changing the ticket status and notification card when bot posts user question to SME channel.
    /// </summary>
    public class SmeTicketCard
    {
        private readonly TicketEntity ticket;
        private readonly List<ExpertEntity> experts;

        /// <summary>
        /// Initializes a new instance of the <see cref="SmeTicketCard"/> class.
        /// </summary>
        /// <param name="ticket">The ticket model with the latest details.</param>
        public SmeTicketCard(TicketEntity ticket)
        {
            this.ticket = ticket;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SmeTicketCard"/> class.
        /// </summary>
        /// <param name="ticket">The ticket model with the latest details.</param>
        /// <param name="experts">Experts details in expert channel.</param>
        public SmeTicketCard(TicketEntity ticket, List<ExpertEntity> experts)
        {
            this.ticket = ticket;
            this.experts = experts;
        }

        /// <summary>
        /// Gets the ticket that is the basis for the information in this card.
        /// </summary>
        protected TicketEntity Ticket => this.ticket;

        /// <summary>
        /// Returns an attachment based on the state and information of the ticket.
        /// </summary>
        /// <param name="localTimestamp">Local timestamp of the user activity.</param>
        /// <param name="appBaseUri">The base URI where the app is hosted.</param>
        /// <returns>Returns the attachment that will be sent in a message.</returns>
        public Attachment ToAttachment(DateTimeOffset? localTimestamp, string appBaseUri)
        {
            List<AdaptiveColumn> columns = new List<AdaptiveColumn>
            {
                new AdaptiveColumn
                {
                    Items = new List<AdaptiveElement>
                    {
                        new AdaptiveImage
                        {
                            Style = AdaptiveImageStyle.Default,
                            Size = AdaptiveImageSize.Medium,
                            Url = new Uri(appBaseUri + "/content/request_channel.png"),
                        },
                    },
                    Width = "auto",
                },
                new AdaptiveColumn
                {
                    Items = new List<AdaptiveElement>
                    {
                        new AdaptiveTextBlock()
                        {
                            Text = this.Ticket.Title,
                            Weight = AdaptiveTextWeight.Bolder,
                            Wrap = true,
                        },
                        new AdaptiveTextBlock()
                        {
                            Text = string.Format(CultureInfo.InvariantCulture, Strings.QuestionForExpertSubHeaderText, this.Ticket.RequesterName),
                            Spacing = AdaptiveSpacing.None,
                            IsSubtle = true,
                            Wrap = true,
                        },
                    },
                    Width = "stretch",
                },
            };

            if (this.ticket.Feedback != null)
            {
                string feedbackEmjio = null;
                if (this.ticket.Feedback == nameof(TicketSatisficationRating.Satisfied))
                {
                    feedbackEmjio = "face_smile.png";
                }
                else if (this.ticket.Feedback == nameof(TicketSatisficationRating.Neutral))
                {
                    feedbackEmjio = "face_straigh.png";
                }
                else if (this.ticket.Feedback == nameof(TicketSatisficationRating.Disappointed))
                {
                    feedbackEmjio = "face_sad.png";
                }

                if (feedbackEmjio != null)
                {
                    columns.Add(new AdaptiveColumn
                    {
                        Items = new List<AdaptiveElement>
                    {
                        new AdaptiveImage
                        {
                            Style = AdaptiveImageStyle.Default,
                            Size = AdaptiveImageSize.Small,
                            Url = new Uri(appBaseUri + "/content/" + feedbackEmjio),
                            HorizontalAlignment = AdaptiveHorizontalAlignment.Right,
                        },
                    },
                        Width = "auto",
                        VerticalContentAlignment = AdaptiveVerticalContentAlignment.Top,
                    });
                }
            }

            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
            {
                Body = new List<AdaptiveElement>
                {
                    new AdaptiveColumnSet
                    {
                        Columns = columns,
                    },
                    new AdaptiveFactSet
                    {
                        Facts = this.BuildFactSet(localTimestamp),
                    },
                },

                Actions = this.BuildActions(appBaseUri),
            };

            return new Attachment
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card,
            };
        }

        /// <summary>
        /// Return the appropriate set of card actions based on the state and information in the ticket.
        /// </summary>
        /// <param name="appBaseUri">The base URI where the app is hosted.</param>
        /// <returns>Adaptive card actions.</returns>
        protected virtual List<AdaptiveAction> BuildActions(string appBaseUri)
        {
            List<AdaptiveAction> actionsList = new List<AdaptiveAction>();

            actionsList.Add(this.CreateChatWithUserAction(appBaseUri));

            actionsList.Add(new AdaptiveShowCardAction
            {
                Title = Strings.ChangeStatusButtonText,
                Card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
                {
                    Actions = this.BuildChangeStatesActions(),
                },
                IconUrl = appBaseUri + "/content/change.png",
            });

            if (!string.IsNullOrEmpty(this.Ticket.KnowledgeBaseAnswer))
            {
                actionsList.Add(new AdaptiveShowCardAction
                {
                    Title = Strings.ViewArticleButtonText,
                    Card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
                    {
                        Body = new List<AdaptiveElement>
                        {
                            new AdaptiveTextBlock
                            {
                                Text = CardHelper.TruncateStringIfLonger(this.Ticket.KnowledgeBaseAnswer, CardHelper.KnowledgeBaseAnswerMaxDisplayLength),
                                Wrap = true,
                            },
                        },
                    },
                    IconUrl = appBaseUri + "/content/article.png",
                });
            }

            return actionsList;
        }

        /// <summary>
        /// Create an adaptive card action that starts a chat with the user.
        /// </summary>
        /// <param name="appBaseUri">The base URI where the app is hosted.</param>
        /// <returns>Adaptive card action for starting chat with user.</returns>
        protected AdaptiveAction CreateChatWithUserAction(string appBaseUri)
        {
            var messageToSend = string.Format(CultureInfo.InvariantCulture, Strings.SMEUserChatMessage, this.Ticket.Title);
            var encodedMessage = Uri.EscapeDataString(messageToSend);

            return new AdaptiveOpenUrlAction
            {
                IconUrl = appBaseUri + "/content/chat.png",
                Title = string.Format(CultureInfo.InvariantCulture, Strings.ChatTextButton, this.Ticket.RequesterGivenName),
                Url = new Uri($"https://teams.microsoft.com/l/chat/0/0?users={Uri.EscapeDataString(this.Ticket.RequesterUserPrincipalName)}&message={encodedMessage}"),
            };
        }

        /// <summary>
        /// Create change state actions.
        /// </summary>
        /// <returns>action list</returns>
        private List<AdaptiveAction> BuildChangeStatesActions()
        {
            List<AdaptiveAction> actionsList = new List<AdaptiveAction>();
            if (this.Ticket.Status == (int)TicketState.UnAssigned)
            {
                actionsList.Add(this.CreateAssignToMeAction());
                actionsList.Add(this.CreateAssignToOthersAction());
            }
            else if (this.Ticket.Status == (int)TicketState.Assigned)
            {
                actionsList.Add(this.CreateResolveAction());
                actionsList.Add(this.CreatePendingAction());
                actionsList.Add(this.CreateAssignToMeAction());
                actionsList.Add(this.CreateAssignToOthersAction());
            }
            else if (this.Ticket.Status == (int)TicketState.Pending)
            {
                actionsList.Add(this.CreateResolveAction());
            }
            else if (this.Ticket.Status == (int)TicketState.Resolved)
            {
                actionsList.Add(this.CreateAssignToMeAction());
                actionsList.Add(this.CreateAssignToOthersAction());
            }

            return actionsList;
        }

        /// <summary>
        /// Create assigne to me action.
        /// </summary>
        /// <returns>action.</returns>
        private AdaptiveAction CreateAssignToMeAction()
        {
            return new AdaptiveSubmitAction
            {
                Title = this.ticket.Status == (int)TicketState.Resolved ? Strings.ReopenAssignToMeActionTitle : Strings.AssignToMeActionTitle,
                Data = new ChangeTicketStatusPayload
                {
                    Action = ChangeTicketStatusPayload.AssignToSelfAction,
                    TicketId = this.ticket.TicketId,
                },
            };
        }

        /// <summary>
        /// Create assigne to others action.
        /// </summary>
        /// <returns>action.</returns>
        private AdaptiveAction CreateAssignToOthersAction()
        {
            return new AdaptiveShowCardAction
            {
                Title = this.ticket.Status == (int)TicketState.Resolved ? Strings.ReopenAssignToOthersActionTitle : Strings.AssignToOthersActionTitle,
                Card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
                {
                    Body = new List<AdaptiveElement>
                    {
                        this.GetAdaptiveChoiceSetInput(),
                    },
                    Actions = new List<AdaptiveAction>
                    {
                        new AdaptiveSubmitAction
                        {
                            Data = new ChangeTicketStatusPayload
                            {
                                Action = ChangeTicketStatusPayload.AssignToOthersAction,
                                TicketId = this.Ticket.TicketId,
                            },
                        },
                    },
                },
            };
        }

        /// <summary>
        /// Create pending action.
        /// </summary>
        /// <returns>action.</returns>
        private AdaptiveAction CreatePendingAction()
        {
            return new AdaptiveShowCardAction
            {
                Title = Strings.PendingActionTitle,
                Card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
                {
                    Body = new List<AdaptiveElement>
                    {
                         new AdaptiveTextBlock
                         {
                             Text = Strings.CommentText,
                             Wrap = true,
                         },
                         new AdaptiveTextInput
                         {
                             Spacing = AdaptiveSpacing.Small,
                             Id = nameof(ChangeTicketStatusPayload.PendingComment),
                             Placeholder = Strings.CommentPlachHonderText,
                             IsMultiline = true,
                         },
                    },
                    Actions = new List<AdaptiveAction>
                    {
                        new AdaptiveSubmitAction
                        {
                            Data = new ChangeTicketStatusPayload
                            {
                                TicketId = this.Ticket.TicketId,
                                Action = ChangeTicketStatusPayload.PendingAction,
                            },
                        },
                    },
                },
            };
        }

        /// <summary>
        /// Create resolve action.
        /// </summary>
        /// <returns>action.</returns>
        private AdaptiveAction CreateResolveAction()
        {
            return new AdaptiveShowCardAction
            {
                Title = Strings.ResolveActionChoiceTitle,
                Card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
                {
                    Body = new List<AdaptiveElement>
                    {
                         new AdaptiveTextBlock
                         {
                             Text = Strings.CommentText,
                             Wrap = true,
                         },
                         new AdaptiveTextInput
                         {
                             Spacing = AdaptiveSpacing.Small,
                             Id = nameof(ChangeTicketStatusPayload.ResolveComment),
                             Placeholder = Strings.CommentPlachHonderText,
                             IsMultiline = true,
                         },
                    },
                    Actions = new List<AdaptiveAction>
                    {
                        new AdaptiveSubmitAction
                        {
                            Data = new ChangeTicketStatusPayload
                            {
                                TicketId = this.Ticket.TicketId,
                                Action = ChangeTicketStatusPayload.ResolveAction,
                            },
                        },
                    },
                },
            };
        }

        /// <summary>
        /// Return the appropriate fact set based on the state and information in the ticket.
        /// </summary>
        /// <param name="localTimestamp">The current timestamp.</param>
        /// <returns>The fact set showing the necessary details.</returns>
        private List<AdaptiveFact> BuildFactSet(DateTimeOffset? localTimestamp)
        {
            List<AdaptiveFact> factList = new List<AdaptiveFact>();

            if (!string.IsNullOrEmpty(this.Ticket.TicketId))
            {
                factList.Add(new AdaptiveFact
                {
                    Title = Strings.TicketIDFact,
                    Value = this.Ticket.TicketId.Substring(0, 8),
                });
            }

            if (!string.IsNullOrEmpty(this.Ticket.Subject))
            {
                factList.Add(new AdaptiveFact
                {
                    Title = Strings.SubjectFact,
                    Value = this.Ticket.Subject,
                });
            }

            if (!string.IsNullOrEmpty(this.Ticket.Description))
            {
                factList.Add(new AdaptiveFact
                {
                    Title = Strings.DescriptionFact,
                    Value = this.Ticket.Description,
                });
            }

            if (!string.IsNullOrEmpty(this.Ticket.UserQuestion))
            {
                factList.Add(new AdaptiveFact
                {
                    Title = Strings.QuestionAskedFactTitle,
                    Value = this.Ticket.UserQuestion,
                });
            }

            factList.Add(new AdaptiveFact
            {
                Title = Strings.StatusFactTitle,
                Value = CardHelper.GetTicketDisplayStatusForSme(this.Ticket),
            });

            if (this.Ticket.Status == (int)TicketState.Pending)
            {
                factList.Add(new AdaptiveFact
                {
                    Title = Strings.CommentText,
                    Value = this.Ticket.PendingComment,
                });
            }

            if (this.Ticket.Status == (int)TicketState.Resolved)
            {
                factList.Add(new AdaptiveFact
                {
                    Title = Strings.ClosedFactTitle,
                    Value = CardHelper.GetFormattedDateInUserTimeZone(this.Ticket.DateClosed.Value, localTimestamp),
                });

                factList.Add(new AdaptiveFact
                {
                    Title = Strings.CommentText,
                    Value = this.Ticket.ResolveComment,
                });
            }

            return factList;
        }

        /// <summary>
        /// Return the appropriate status choices based on the state and information in the ticket.
        /// </summary>
        /// <returns>An adaptive element which contains the dropdown choices.</returns>
        private AdaptiveChoiceSetInput GetAdaptiveChoiceSetInput()
        {
            AdaptiveChoiceSetInput choiceSet = new AdaptiveChoiceSetInput
            {
                Id = nameof(ChangeTicketStatusPayload.OtherAssigneeInfo),
                IsMultiSelect = false,
                Style = AdaptiveChoiceInputStyle.Compact,
            };

            List<AdaptiveChoice> choices = new List<AdaptiveChoice>();
            foreach (var expert in this.experts)
            {
                choices.Add(new AdaptiveChoice()
                {
                    Title = expert.Name,
                    Value = $"{expert.Name}:{expert.ID}",
                });
            }

            choiceSet.Choices = choices;
            if (this.experts.Count > 0)
            {
                choiceSet.Value = $"{this.experts[0].Name}:{this.experts[0].ID}";
            }

            return choiceSet;
        }
    }
}