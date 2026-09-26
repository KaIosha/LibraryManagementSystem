using System;
using System.Collections.Generic;
using System.Text;
using LibraryManagementSystem.Application.DTOs;

namespace LibraryManagementSystem.Application.Interfaces.IServices
{
    public interface IEmailService
    {
        Task SendAsync(string to, string subject, string Body);
    }
}
