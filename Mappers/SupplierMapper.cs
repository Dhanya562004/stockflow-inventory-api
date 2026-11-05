using InventoryManagementSystemAPI.DTOs.Supplier;
using InventoryManagementSystemAPI.Models;

namespace InventoryManagementSystemAPI.Mappers
{
    public static class SupplierMapper
    {
        public static SupplierDto ToDto(this Supplier supplier) 
        {
            return new SupplierDto 
            {
                Id = supplier.Id,
                CompanyName = supplier.CompanyName,
                ContactName = supplier.ContactName,
                Email = supplier.Email,
                PhoneNumber = supplier.PhoneNumber,
                Address = supplier.Address,
                City = supplier.City,
                State = supplier.State,
                ZipCode = supplier.ZipCode
            };
        }

        public static Supplier ToEntity(this CreateSupplierDto dto) 
        {
            return new Supplier 
            {
                CompanyName = dto.CompanyName,
                ContactName = dto.ContactName,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Address = dto.Address,
                City = dto.City,
                State = dto.State,
                ZipCode = dto.ZipCode
            };
        }

        public static void UpdateEntity(this UpdateSupplierDto dto, Supplier supplier) 
        {
            supplier.CompanyName = dto.CompanyName;
            supplier.ContactName = dto.ContactName;
            supplier.Email = dto.Email;
            supplier.PhoneNumber = dto.PhoneNumber;
            supplier.Address = dto.Address;
            supplier.City = dto.City;
            supplier.State = dto.State;
            supplier.ZipCode = dto.ZipCode;
        }

        public static List<SupplierDto> ToDtoList(this IEnumerable<Supplier> suppliers) 
        {
            return suppliers
                .Select(s => s.ToDto())
                .ToList();
        }
    }
}
