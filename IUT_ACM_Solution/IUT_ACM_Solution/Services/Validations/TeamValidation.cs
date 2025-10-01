using IUT_ACM_Solution.Interfaces.Repositories;
using IUT_ACM_Solution.Interfaces.Validations;
using IUT_ACM_Solution.Models;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace IUT_ACM_Solution.Services.Validations
{
    public class TeamValidation : ITeamValidation
    {
        private const string EMAIL_PATTERN = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        private readonly ITeamRepository teamRepository;
        private readonly UserManager<User> userManager;
        private Event @event;

        public TeamValidation(ITeamRepository teamRepository, UserManager<User> userManager)
        {
            this.teamRepository = teamRepository;
            this.userManager = userManager;
        }

        public async Task<IValidationResult> ValidateTeam(Team team)
        {
            if (team == null)
            {
                return ValidationResult.Invalid("Team cannot be null");
            }

            // Validate using data annotations
            var annotationValidation = ValidateDataAnnotations(team);
            if (!annotationValidation.IsValid)
            {
                return annotationValidation;
            }

            // Team name validations
            var teamNameValidation = ValidateTeamName(team);
            if (!teamNameValidation.IsValid)
            {
                return teamNameValidation;
            }

            // Team members validations
            var membersValidation = await ValidateTeamMembers(team);
            if (!membersValidation.IsValid)
            {
                return membersValidation;
            }

            // Email validations
            var emailValidation = ValidateEmails(team);
            if (!emailValidation.IsValid)
            {
                return emailValidation;
            }

            // Event association validation
            var eventValidation = ValidateEventAssociation(team);
            if (!eventValidation.IsValid)
            {
                return eventValidation;
            }

            // Registration date validation
            var dateValidation = ValidateRegistrationDate(team);
            if (!dateValidation.IsValid)
            {
                return dateValidation;
            }

            return ValidationResult.Valid();
        }

        public async Task<IValidationResult> ValidateTeamCreationAsync(Team team)
        {
            // Run all basic validations
            var basicValidation = await ValidateTeam(team);
            if (!basicValidation.IsValid)
            {
                return basicValidation;
            }

            // Check the team name to be unique 
            foreach(var t in team.Event.ParticipatingTeams)
            {
                if (t.TeamName == team.TeamName)
                {
                    return ValidationResult.Invalid("There is already a team with this name");
                }
            }

            // Creation-specific validations
            if (team.Id != Guid.Empty)
            {
                return ValidationResult.Invalid("Team ID should not be set for new teams. It will be generated automatically");
            }

            if (team.HasPaid)
            {
                return ValidationResult.Invalid("New teams cannot be marked as paid during creation");
            }

            if (team.RegisteredAt > DateTime.UtcNow)
            {
                return ValidationResult.Invalid("Registration date cannot be in the future");
            }

            // Validate team leader email is present
            if (string.IsNullOrWhiteSpace(team.TeamLeaderEmail))
            {
                return ValidationResult.Invalid("Team must have at least one member (team leader)");
            }

            // Validate Event exists (would need repository in real implementation)
            if (team.EventId == Guid.Empty)
            {
                return ValidationResult.Invalid("Team must be associated with a valid event");
            }

            // Simulate async database checks (e.g., checking for duplicate team names per event)
            await Task.CompletedTask;

            return ValidationResult.Valid();
        }

        public async Task<IValidationResult> ValidateTeamUpdateAsync(Team team)
        {
            // Run all basic validations
            var basicValidation = await ValidateTeam(team);
            if (!basicValidation.IsValid)
            {
                return basicValidation;
            }

            // Update-specific validations
            if (team.Id == Guid.Empty)
            {
                return ValidationResult.Invalid("Team ID is required for updates");
            }

            // Cannot change EventId after creation
            // This would need to be checked against the existing team in database
            // For now, just validate it exists
            if (team.EventId == Guid.Empty)
            {
                return ValidationResult.Invalid("Cannot remove event association from team");
            }

            // Validate payment status changes
            if (team.HasPaid && team.FeeToPay > 0 && string.IsNullOrWhiteSpace(team.Authority))
            {
                return ValidationResult.Invalid("Payment authority/reference is required when marking team as paid");
            }

            // Cannot mark as paid if fee is 0 and HasPaid is true
            if (team.HasPaid && team.FeeToPay == 0)
            {
                // This is actually OK - free events can be "paid" (confirmed)
            }

            // Validate registration date hasn't been moved to future
            if (team.RegisteredAt > DateTime.UtcNow)
            {
                return ValidationResult.Invalid("Registration date cannot be set to the future");
            }

            // Simulate async database checks
            await Task.CompletedTask;

            return ValidationResult.Valid();
        }
        private IValidationResult ValidateDataAnnotations(Team team)
        {
            var validationContext = new ValidationContext(team);
            var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();

            if (!Validator.TryValidateObject(team, validationContext, validationResults, true))
            {
                var errors = string.Join("; ", validationResults.Select(r => r.ErrorMessage));
                return ValidationResult.Invalid($"Data annotation validation failed: {errors}");
            }

            return ValidationResult.Valid();
        }

        private IValidationResult ValidateTeamName(Team team)
        {
            if (string.IsNullOrWhiteSpace(team.TeamName))
            {
                return ValidationResult.Invalid("Team name is required and cannot be empty");
            }

            // Check for valid characters (alphanumeric, spaces, common symbols)
            if (!Regex.IsMatch(team.TeamName, @"^[a-zA-Z0-9\s\-_\.]+$"))
            {
                return ValidationResult.Invalid("Team name can only contain letters, numbers, spaces, hyphens, underscores, and periods");
            }

            return ValidationResult.Valid();
        }

        private async Task<IValidationResult> ValidateTeamMembers(Team team)
        {
            if (team.TeamMemberEmails == null)
            {
                return ValidationResult.Invalid("Team member emails list cannot be null");
            }

            if (team.TeamMemberEmails.Count > @event.MaxTeamMembers)
            {
                return ValidationResult.Invalid($"Team cannot have more than {team.Event?.MaxTeamMembers} members");
            }

            // Check for empty or whitespace emails
            if (team.TeamMemberEmails.Any(email => string.IsNullOrWhiteSpace(email)))
            {
                return ValidationResult.Invalid("Team member emails cannot be empty or whitespace");
            }

            // Check for duplicate emails (case-insensitive)
            var uniqueEmails = team.TeamMemberEmails
                .Select(e => e.Trim().ToLowerInvariant())
                .ToList();

            foreach (var email in team.TeamMemberEmails)
            {
                var contestant = await userManager.FindByEmailAsync(email);
                if (contestant == null)
                {
                    return ValidationResult.Invalid("All team members must have an account");
                }
            }

            if (uniqueEmails.Count != uniqueEmails.Distinct().Count())
            {
                return ValidationResult.Invalid("Team cannot have duplicate member emails");
            }

            return ValidationResult.Valid();
        }

        private IValidationResult ValidateEmails(Team team)
        {
            if (!team.HasValidEmails)
            {
                return ValidationResult.Invalid("All team member emails must be valid email addresses");
            }

            // More rigorous email validation
            foreach (var email in team.TeamMemberEmails)
            {
                var trimmedEmail = email.Trim();

                // Basic format check
                if (!Regex.IsMatch(trimmedEmail, EMAIL_PATTERN, RegexOptions.IgnoreCase))
                {
                    return ValidationResult.Invalid($"Invalid email format: {email}");
                }

                // Check email length
                if (trimmedEmail.Length > 254) // RFC 5321
                {
                    return ValidationResult.Invalid($"Email address too long: {email}");
                }

                // Check for common invalid patterns
                if (trimmedEmail.StartsWith(".") || trimmedEmail.EndsWith("."))
                {
                    return ValidationResult.Invalid($"Email cannot start or end with a period: {email}");
                }

                if (trimmedEmail.Contains(".."))
                {
                    return ValidationResult.Invalid($"Email cannot contain consecutive periods: {email}");
                }

                // Validate domain part
                var parts = trimmedEmail.Split('@');
                if (parts.Length == 2)
                {
                    var domain = parts[1];
                    if (domain.Length < 3 || !domain.Contains("."))
                    {
                        return ValidationResult.Invalid($"Invalid email domain: {email}");
                    }
                }
            }

            // Validate team leader email specifically
            if (string.IsNullOrWhiteSpace(team.TeamLeaderEmail))
            {
                return ValidationResult.Invalid("Team must have a valid team leader email (first member)");
            }

            return ValidationResult.Valid();
        }

        private IValidationResult ValidateEventAssociation(Team team)
        {
            if (team.EventId == Guid.Empty)
            {
                return ValidationResult.Invalid("Team must be associated with a valid event");
            }

            // Additional validation if Event navigation property is loaded
            if (team.Event != null)
            {
                // Validate team size against event constraints
                if (team.Event.IsTeamBased)
                {
                    if (team.TeamMemberEmails.Count > team.Event.MaxTeamMembers)
                    {
                        return ValidationResult.Invalid(
                            $"Team has {team.TeamMemberEmails.Count} members but event allows maximum {team.Event.MaxTeamMembers} members per team");
                    }
                }
                else
                {
                    return ValidationResult.Invalid("Cannot create team for non-team-based event");
                }

                // Validate event status
                if (team.Event.Status == EventStatus.Completed)
                {
                    return ValidationResult.Invalid("Cannot register team for completed event");
                }

                if (team.Event.Status == EventStatus.Cancelled)
                {
                    return ValidationResult.Invalid("Cannot register team for cancelled event");
                }

                // Validate event capacity
                if (!team.Event.HasCapacityLeft)
                {
                    return ValidationResult.Invalid("Event has reached maximum capacity");
                }
            }

            return ValidationResult.Valid();
        }

        private IValidationResult ValidateRegistrationDate(Team team)
        {
            if (team.RegisteredAt > DateTime.UtcNow)
            {
                return ValidationResult.Invalid("Registration date cannot be in the future");
            }

            // Check if registration is too far in the past (optional business rule)
            if (team.RegisteredAt < DateTime.UtcNow.AddYears(-2))
            {
                return ValidationResult.Invalid("Registration date is too far in the past (more than 2 years)");
            }

            // If Event is loaded, validate registration is after event creation
            if (team.Event != null)
            {
                if (team.RegisteredAt < team.Event.CreatedAt)
                {
                    return ValidationResult.Invalid("Team cannot be registered before event was created");
                }

                // Validate registration is before event end date
                if (team.RegisteredAt > team.Event.EndDate)
                {
                    return ValidationResult.Invalid("Team cannot be registered after event has ended");
                }
            }

            return ValidationResult.Valid();
        }

        public IValidationResult ValidateTeamMemberAddition(Team team, string newMemberEmail)
        {
            if (string.IsNullOrWhiteSpace(newMemberEmail))
            {
                return ValidationResult.Invalid("Member email cannot be empty");
            }

            if (team.TeamMemberEmails.Contains(newMemberEmail, StringComparer.OrdinalIgnoreCase))
            {
                return ValidationResult.Invalid($"Member with email {newMemberEmail} is already in the team");
            }

            if (team.TeamMemberEmails.Count >= team.Event?.MaxTeamMembers)
            {
                return ValidationResult.Invalid($"Team already has maximum {team.Event?.MaxTeamMembers} members");
            }

            // Validate email format
            if (!Regex.IsMatch(newMemberEmail.Trim(), EMAIL_PATTERN, RegexOptions.IgnoreCase))
            {
                return ValidationResult.Invalid($"Invalid email format: {newMemberEmail}");
            }

            return ValidationResult.Valid();
        }

        public IValidationResult ValidateTeamMemberRemoval(Team team, string memberEmail)
        {
            if (team.TeamMemberEmails.Count <= 1)
            {
                return ValidationResult.Invalid(
                    $"Cannot remove member. Team must have at least 1 member");
            }

            if (!team.TeamMemberEmails.Contains(memberEmail, StringComparer.OrdinalIgnoreCase))
            {
                return ValidationResult.Invalid($"Member with email {memberEmail} is not in the team");
            }

            // Prevent removing team leader
            if (string.Equals(memberEmail, team.TeamLeaderEmail, StringComparison.OrdinalIgnoreCase))
            {
                return ValidationResult.Invalid("Cannot remove team leader. Transfer leadership first or delete the team");
            }

            return ValidationResult.Valid();
        }

        public IValidationResult ValidatePaymentConfirmation(Team team, string authority)
        {
            if (team.HasPaid)
            {
                return ValidationResult.Invalid("Team payment has already been confirmed");
            }

            if (team.FeeToPay <= 0)
            {
                return ValidationResult.Invalid("Cannot confirm payment for team with no fee");
            }

            if (string.IsNullOrWhiteSpace(authority))
            {
                return ValidationResult.Invalid("Payment authority/reference is required for payment confirmation");
            }

            if (authority.Length > 200)
            {
                return ValidationResult.Invalid("Payment authority/reference cannot exceed 200 characters");
            }

            return ValidationResult.Valid();
        }
    }
}