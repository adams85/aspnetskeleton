using System;
using System.Web.Security;

namespace AspNetSkeleton.UI.Infrastructure.Security
{
    public class UIMembershipProvider : MembershipProvider
    {
        #region Properties

        public override string ApplicationName
        {
            get => typeof(MvcApplication).Assembly.GetName().Name;
            set { throw new NotSupportedException(); }
        }

        public override int MaxInvalidPasswordAttempts => throw new NotSupportedException();

        public override int MinRequiredNonAlphanumericCharacters => throw new NotSupportedException();

        public override int MinRequiredPasswordLength => throw new NotSupportedException();

        public override int PasswordAttemptWindow => throw new NotSupportedException();

        public override MembershipPasswordFormat PasswordFormat => throw new NotSupportedException();

        public override string PasswordStrengthRegularExpression => throw new NotSupportedException();

        public override bool RequiresUniqueEmail => throw new NotSupportedException();

        public override bool EnablePasswordRetrieval => throw new NotSupportedException();

        public override bool EnablePasswordReset => throw new NotSupportedException();

        public override bool RequiresQuestionAndAnswer => throw new NotSupportedException();
        #endregion

        #region Functions        
        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            throw new NotSupportedException();
        }

        public override bool ValidateUser(string username, string password)
        {
            throw new NotSupportedException();
        }

        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            throw new NotSupportedException();
        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            throw new NotSupportedException();
        }

        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            throw new NotSupportedException();
        }

        public override bool UnlockUser(string userName)
        {
            throw new NotSupportedException();
        }

        public override int GetNumberOfUsersOnline()
        {
            throw new NotSupportedException();
        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            throw new NotSupportedException();
        }

        public override string GetUserNameByEmail(string email)
        {
            throw new NotSupportedException();
        }

        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotSupportedException();
        }

        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotSupportedException();
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotSupportedException();
        }

        public override string GetPassword(string username, string answer)
        {
            throw new NotSupportedException();
        }

        public override string ResetPassword(string username, string answer)
        {
            throw new NotSupportedException();
        }

        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            throw new NotSupportedException();
        }

        public override void UpdateUser(MembershipUser user)
        {
            throw new NotSupportedException();
        }

        #endregion
    }
}