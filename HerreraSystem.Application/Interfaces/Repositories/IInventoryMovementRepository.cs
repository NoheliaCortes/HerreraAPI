using HerreraSystem.Application.DTOs.InventoryMovementDtos;
using HerreraSystem.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.Interfaces.Repositories
{
    public interface IInventoryMovementRepository
    {
        Task<InventoryMovement> CreateAsync(InventoryMovement movement);

        


    }
}
