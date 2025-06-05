using EventTracking.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace EventTracking.Components.Account
{
    internal sealed class IdentityNoOpEmailSender : IEmailSender<ApplicationUser>
    {
        // Gán trực tiếp email và mật khẩu ứng dụng ở đây
        private readonly string fromEmail = "event.tracking2025@gmail.com";
        private readonly string appPassword = "nmqu kwtc ynor vfvy";

        private async Task SendAsync(string toEmail, string subject, string htmlMessage)
        {
            var mail = new MailMessage
            {
                From = new MailAddress(fromEmail),
                Subject = subject,
                Body = htmlMessage,
                IsBodyHtml = true
            };
            mail.To.Add(toEmail);

            using var smtp = new SmtpClient("smtp.gmail.com", 587)
            {
                Credentials = new NetworkCredential(fromEmail, appPassword),
                EnableSsl = true
            };

            try
            {
                await smtp.SendMailAsync(mail);
            }
            catch (Exception ex)
            {
                // Ghi log hoặc ném lỗi tùy theo nhu cầu
                throw new InvalidOperationException("Gửi email thất bại", ex);
            }
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage) =>
            SendAsync(email, subject, htmlMessage);

        public Task SendConfirmationLinkAsync(ApplicationUser user, string email, string confirmationLink) =>
            SendEmailAsync(email, "Xác nhận email", $"Vui lòng <a href='{confirmationLink}'>bấm vào đây để xác nhận</a>.");

        public Task SendPasswordResetLinkAsync(ApplicationUser user, string email, string resetLink) =>
            SendEmailAsync(email, "Đặt lại mật khẩu", $"Vui lòng <a href='{resetLink}'>bấm vào đây để đặt lại mật khẩu</a>.");

        public Task SendPasswordResetCodeAsync(ApplicationUser user, string email, string resetCode) =>
            SendEmailAsync(email, "Mã đặt lại mật khẩu", $"Mã đặt lại mật khẩu của bạn là: <strong>{resetCode}</strong>");
    }
}