using ApplicationAuth.Common.Exceptions;
using ApplicationAuth.DAL.Abstract;
using ApplicationAuth.Domain.Entities.Identity;
using ApplicationAuth.Domain.Entities.Telegram;
using ApplicationAuth.Models.RequestModels.Telegram;
using ApplicationAuth.Models.ResponseModels.Saldo;
using ApplicationAuth.Models.ResponseModels.Telegram;
using ApplicationAuth.Services.Interfaces;
using ApplicationAuth.Services.Interfaces.Telegram;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Telegram.Bot.Types;

namespace ApplicationAuth.Services.Services.Telegram
{
    public class TelegramService : ITelegramService
    {
        private readonly IMapper _mapper = null;
        private readonly IAccountService _accountService;
        private readonly ISaldoService _saldoService;
        private readonly IUnitOfWork _unitOfWork;//*-*-*-*--*

        public TelegramService(
            IMapper mapper,
            IAccountService accountService,
            ISaldoService saldoService,
            IUnitOfWork unitOfWork//*-*-*-*-*-*-*
            )
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _accountService = accountService;
            _saldoService = saldoService;
        }
    } 
}
