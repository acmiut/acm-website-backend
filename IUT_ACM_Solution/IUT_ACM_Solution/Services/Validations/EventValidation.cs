using IUT_ACM_Solution.Interfaces.Repositories;
using IUT_ACM_Solution.Interfaces.Validations;
using IUT_ACM_Solution.Models;
using System.ComponentModel.DataAnnotations;

namespace IUT_ACM_Solution.Services.Validations
{
    public class EventValidation : IEventValidation
    {
        private const int MIN_EVENT_DURATION_HOURS = 1;
        private const int MAX_EVENT_DURATION_DAYS = 365;
        private const int MIN_TEAM_SIZE = 1;
        private const int MAX_TEAM_SIZE = 50;
        private readonly IEventRepository eventRepository;

        public EventValidation(IEventRepository eventRepository)
        {
            this.eventRepository = eventRepository;
        }

        public IValidationResult ValidateEvent(Event @event)
        {
            if (@event == null)
            {
                return ValidationResult.Invalid("Event cannot be null");
            }

            // Validate using data annotations first
            var annotationValidation = ValidateDataAnnotations(@event);
            if (!annotationValidation.IsValid)
            {
                return annotationValidation;
            }

            // Core information validations
            var coreValidation = ValidateCoreInformation(@event);
            if (!coreValidation.IsValid)
            {
                return coreValidation;
            }

            // Date and time validations
            var dateValidation = ValidateDates(@event);
            if (!dateValidation.IsValid)
            {
                return dateValidation;
            }

            // Capacity and participants validations
            var capacityValidation = ValidateCapacity(@event);
            if (!capacityValidation.IsValid)
            {
                return capacityValidation;
            }

            // Team settings validations
            var teamValidation = ValidateTeamSettings(@event);
            if (!teamValidation.IsValid)
            {
                return teamValidation;
            }

            // Financial validations
            var financialValidation = ValidateFinancials(@event);
            if (!financialValidation.IsValid)
            {
                return financialValidation;
            }

            // Event type and location validations
            var logisticsValidation = ValidateLogistics(@event);
            if (!logisticsValidation.IsValid)
            {
                return logisticsValidation;
            }

            return ValidationResult.Valid();
        }

        public async Task<IValidationResult> ValidateEventCreationAsync(Event @event)
        {
            // Run all basic validations
            var basicValidation = ValidateEvent(@event);
            if (!basicValidation.IsValid)
            {
                return basicValidation;
            }

            // Creation-specific validations
            if (@event.Id != Guid.Empty)
            {
                return ValidationResult.Invalid("Event ID should not be set for new events. It will be generated automatically");
            }

            if (@event.CurrentParticipants != 0)
            {
                return ValidationResult.Invalid("New events must start with 0 participants");
            }

            if (@event.Status != EventStatus.Upcoming)
            {
                return ValidationResult.Invalid("New events must have 'Upcoming' status");
            }

            if (@event.UserEvents?.Any() == true)
            {
                return ValidationResult.Invalid("New events cannot have existing user registrations");
            }

            if (@event.ParticipatingTeams?.Any() == true)
            {
                return ValidationResult.Invalid("New events cannot have existing participating teams");
            }

            var exists = await eventRepository.ExistsEventByTitleAndDateAsync(@event.Title, @event.StartDate, @event.EndDate);
            if (exists)
            {
                return ValidationResult.Invalid("A similar event with the same title and date already exists.");
            }

            // Simulate async database checks
            await Task.CompletedTask;

            return ValidationResult.Valid();
        }

        public async Task<IValidationResult> ValidateEventUpdateAsync(Event @event)
        {
            // Run all basic validations
            var basicValidation = ValidateEvent(@event);
            if (!basicValidation.IsValid)
            {
                return basicValidation;
            }

            // Update-specific validations
            if (@event.Id == Guid.Empty)
            {
                return ValidationResult.Invalid("Event ID is required for updates");
            }

            // Validate status transitions
            var statusValidation = ValidateStatusTransition(@event);
            if (!statusValidation.IsValid)
            {
                return statusValidation;
            }

            // Don't allow reducing capacity below current participants
            if (@event.CurrentParticipants > @event.Capacity)
            {
                return ValidationResult.Invalid($"Cannot reduce capacity to {@event.Capacity} when there are already {@event.CurrentParticipants} participants");
            }

            // Validate completed events
            if (@event.Status == EventStatus.Completed)
            {
                if (@event.EndDate > DateTime.UtcNow)
                {
                    return ValidationResult.Invalid("Completed events must have end date in the past");
                }
            }

            // Validate ongoing events
            if (@event.Status == EventStatus.Ongoing)
            {
                var now = DateTime.UtcNow;
                if (@event.StartDate > now)
                {
                    return ValidationResult.Invalid("Ongoing events must have started");
                }
                if (@event.EndDate < now)
                {
                    return ValidationResult.Invalid("Ongoing events cannot have already ended");
                }
            }

            var exists = await eventRepository.ExistsEventByTitleAndDateAsync(@event.Title, @event.StartDate, @event.EndDate);
            if (exists)
            {
                return ValidationResult.Invalid("A similar event with the same title and date already exists.");
            }

            // Simulate async database checks
            await Task.CompletedTask;

            return ValidationResult.Valid();
        }
        private IValidationResult ValidateDataAnnotations(Event @event)
        {
            var validationContext = new ValidationContext(@event);
            var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();

            if (!Validator.TryValidateObject(@event, validationContext, validationResults, true))
            {
                var errors = string.Join("; ", validationResults.Select(r => r.ErrorMessage));
                return ValidationResult.Invalid($"Data annotation validation failed: {errors}");
            }

            return ValidationResult.Valid();
        }

        private IValidationResult ValidateCoreInformation(Event @event)
        {
            if (string.IsNullOrWhiteSpace(@event.Title))
            {
                return ValidationResult.Invalid("Event title is required and cannot be empty");
            }

            if (@event.Title.Length < 3)
            {
                return ValidationResult.Invalid("Event title must be at least 3 characters long");
            }

            if (!string.IsNullOrWhiteSpace(@event.ImageUrl))
            {
                if (!Uri.TryCreate(@event.ImageUrl, UriKind.Absolute, out var uri))
                {
                    return ValidationResult.Invalid("Event image URL is not a valid URL format");
                }

                var validSchemes = new[] { "http", "https" };
                if (!validSchemes.Contains(uri.Scheme.ToLower()))
                {
                    return ValidationResult.Invalid("Event image URL must use HTTP or HTTPS protocol");
                }
            }

            return ValidationResult.Valid();
        }

        private IValidationResult ValidateDates(Event @event)
        {
            if (!@event.IsValidDateRange)
            {
                return ValidationResult.Invalid("Event end date must be after start date");
            }

            return ValidationResult.Valid();
        }

        private IValidationResult ValidateCapacity(Event @event)
        {
            if (@event.Capacity < 1)
            {
                return ValidationResult.Invalid("Event capacity must be at least 1");
            }

            if (@event.Capacity > 10000)
            {
                return ValidationResult.Invalid("Event capacity cannot exceed 10,000 participants");
            }

            if (@event.CurrentParticipants < 0)
            {
                return ValidationResult.Invalid("Current participants cannot be negative");
            }

            if (@event.CurrentParticipants > @event.Capacity)
            {
                return ValidationResult.Invalid($"Current participants ({@event.CurrentParticipants}) cannot exceed event capacity ({@event.Capacity})");
            }

            return ValidationResult.Valid();
        }

        private IValidationResult ValidateTeamSettings(Event @event)
        {
            if (@event.IsTeamBased)
            {
                if (@event.MaxTeamMembers < MIN_TEAM_SIZE)
                {
                    return ValidationResult.Invalid($"Team-based events must allow at least {MIN_TEAM_SIZE} member per team");
                }

                if (@event.MaxTeamMembers > MAX_TEAM_SIZE)
                {
                    return ValidationResult.Invalid($"Maximum team size cannot exceed {MAX_TEAM_SIZE} members");
                }

                // Validate that capacity makes sense for team-based events
                if (@event.Capacity < @event.MaxTeamMembers)
                {
                    return ValidationResult.Invalid("Event capacity should be at least equal to max team members for team-based events");
                }
            }

            return ValidationResult.Valid();
        }

        private IValidationResult ValidateFinancials(Event @event)
        {
            if (@event.Fee < 0)
            {
                return ValidationResult.Invalid("Event fee cannot be negative");
            }

            // Validate IsFree property consistency
            var expectedIsFree = @event.Fee <= 0;
            if (@event.IsFree != expectedIsFree)
            {
                return ValidationResult.Invalid($"IsFree property is inconsistent with Fee amount. Expected: {expectedIsFree}, Actual: {@event.IsFree}");
            }

            return ValidationResult.Valid();
        }

        private IValidationResult ValidateLogistics(Event @event)
        {
            // Validate event type specific requirements
            if (@event.EventType == EventType.Offline || @event.EventType == EventType.InPerson)
            {
                if (string.IsNullOrWhiteSpace(@event.Location))
                {
                    return ValidationResult.Invalid("Location is required for offline and in-person events");
                }

                if (@event.Location.Length < 5)
                {
                    return ValidationResult.Invalid("Location must be at least 5 characters long");
                }
            }

            // Validate enum values
            if (!Enum.IsDefined(typeof(EventType), @event.EventType))
            {
                return ValidationResult.Invalid("Invalid event type specified");
            }

            if (!Enum.IsDefined(typeof(EventStatus), @event.Status))
            {
                return ValidationResult.Invalid("Invalid event status specified");
            }

            return ValidationResult.Valid();
        }

        private IValidationResult ValidateStatusTransition(Event @event)
        {
            // This method validates if the current status makes sense
            // For a complete implementation, you'd need the previous status
            var now = DateTime.UtcNow;

            switch (@event.Status)
            {
                case EventStatus.Upcoming:
                    if (@event.StartDate <= now && @event.EndDate > now)
                    {
                        return ValidationResult.Invalid("Event should be 'Ongoing' status as it has already started");
                    }
                    if (@event.EndDate <= now)
                    {
                        return ValidationResult.Invalid("Event should not be 'Upcoming' as it has already ended");
                    }
                    break;

                case EventStatus.Ongoing:
                    if (@event.StartDate > now)
                    {
                        return ValidationResult.Invalid("Event cannot be 'Ongoing' as it hasn't started yet");
                    }
                    if (@event.EndDate <= now)
                    {
                        return ValidationResult.Invalid("Event should be 'Completed' as it has already ended");
                    }
                    break;

                case EventStatus.Completed:
                    if (@event.EndDate > now)
                    {
                        return ValidationResult.Invalid("Event cannot be 'Completed' as the end date is in the future");
                    }
                    break;

                case EventStatus.Cancelled:
                    // Cancelled events can be in any time state
                    break;

                default:
                    return ValidationResult.Invalid("Unknown event status");
            }

            return ValidationResult.Valid();
        }
    }
}