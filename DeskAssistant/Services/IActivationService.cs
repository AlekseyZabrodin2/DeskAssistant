using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeskAssistant.Services
{
    public interface IActivationService
    {
        Task ActivateAsync(object activationArgs);
    }
}
