﻿using AutoMapper;
using Microsoft.Extensions.Logging;
using Registry.Business.Abstraction;
using Registry.Repository.Abstraction;
using Registry.Repository.Model;
using Registry.Shared;

namespace Registry.Business
{
    public class Business : IBusiness
    {
        private readonly IRepository _repository;
        private readonly ILogger<Business> _logger;
        private readonly IMapper _mapper;

        public Business(IRepository repository, ILogger<Business> logger, IMapper mapper)
        {
            _repository = repository;
            _logger = logger;
            _mapper = mapper;
        }

        public async Task CreateCustomer(CustomerInsertDto customerInsertDto, CancellationToken cancellationToken = default)
        {
            var customer = _mapper.Map<Customer>(customerInsertDto);

            await _repository.CreateCustomer(customer, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Customer created successfully.");
        }

        public async Task<CustomerReadDto?> GetCustomerById(int id, CancellationToken cancellationToken = default)
        {
            var customer = await _repository.GetCustomerById(id, cancellationToken);
            if (customer == null) return null;

            var customerReadDto = _mapper.Map<CustomerReadDto>(customer);
            return customerReadDto;
        }

        public async Task<List<CustomerReadDto>> GetAllCustomers(CancellationToken cancellationToken = default)
        {
            var customers = await _repository.GetAllCustomers(cancellationToken);
            if (customers == null || customers.Any() == false) return new List<CustomerReadDto>();

            var customersReadDto = _mapper.Map<List<CustomerReadDto>>(customers);
            return customersReadDto;
        }

        public async Task CreateSupplier(SupplierInsertDto supplierInsertDto, CancellationToken cancellationToken = default)
        {
            var supplier = _mapper.Map<Supplier>(supplierInsertDto);

            await _repository.CreateSupplier(supplier, cancellationToken);
            await _repository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Supplier created successfully.");
        }

        public async Task<SupplierReadDto?> GetSupplierById(int id, CancellationToken cancellationToken = default)
        {
            var supplier = await _repository.GetSupplierById(id, cancellationToken);
            if (supplier == null) return null;

            var supplierReadDto = _mapper.Map<SupplierReadDto>(supplier);
            return supplierReadDto;
        }

        public async Task<List<SupplierReadDto>> GetAllSuppliers(CancellationToken cancellationToken = default)
        {
            var suppliers = await _repository.GetAllSuppliers(cancellationToken);
            if (suppliers == null || suppliers.Any() == false) return new List<SupplierReadDto>();

            var suppliersReadDto = _mapper.Map<List<SupplierReadDto>>(suppliers);
            return suppliersReadDto;
        }
    }
}