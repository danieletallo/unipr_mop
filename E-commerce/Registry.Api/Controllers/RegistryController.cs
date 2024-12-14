using Microsoft.AspNetCore.Mvc;
using Registry.Business.Abstraction;
using Registry.Shared;

namespace Registry.Api.Controllers
{
    [ApiController]
    [Route("[controller]/[action]")]
    public class RegistryController : ControllerBase
    {
        private readonly IBusiness _business;
        private readonly ILogger<RegistryController> _logger;

        public RegistryController(IBusiness business, ILogger<RegistryController> logger)
        {
            _business = business;
            _logger = logger;
        }

        [HttpPost(Name = "CreateCustomer")]
        public async Task<ActionResult> CreateCustomer(CustomerInsertDto customerInsertDto, CancellationToken cancellationToken = default)
        {
            await _business.CreateCustomer(customerInsertDto, cancellationToken);

            return Ok("Customer created successfully!");
        }

        [HttpGet(Name = "ReadCustomer")]
        public async Task<ActionResult<CustomerReadDto?>> ReadCustomer(int id, CancellationToken cancellationToken = default)
        {
            var customer = await _business.GetCustomerById(id, cancellationToken);

            if (customer == null)
            {
                var error = $"Customer with ID {id} not found.";
                _logger.LogWarning(error);
                return NotFound(new { error });
            }

            return new JsonResult(customer);
        }

        [HttpGet(Name = "GetAllCustomers")]
        public async Task<ActionResult<List<CustomerReadDto>>> GetAllCustomers(CancellationToken cancellationToken = default)
        {
            var customers = await _business.GetAllCustomers(cancellationToken);

            return new JsonResult(customers);
        }

        [HttpPost(Name = "CreateSupplier")]
        public async Task<ActionResult> CreateSupplier(SupplierInsertDto supplierInsertDto, CancellationToken cancellationToken = default)
        {
            await _business.CreateSupplier(supplierInsertDto, cancellationToken);

            return Ok("Supplier created successfully!");
        }

        [HttpGet(Name = "ReadSupplier")]
        public async Task<ActionResult<SupplierReadDto?>> ReadSupplier(int id, CancellationToken cancellationToken = default)
        {
            var supplier = await _business.GetSupplierById(id, cancellationToken);

            if (supplier == null)
            {
                var error = $"Supplier with ID {id} not found.";
                _logger.LogWarning(error);
                return NotFound(new { error });
            }

            return new JsonResult(supplier);
        }

        [HttpGet(Name = "GetAllSuppliers")]
        public async Task<ActionResult<List<SupplierReadDto>>> GetAllSuppliers(CancellationToken cancellationToken = default)
        {
            var suppliers = await _business.GetAllSuppliers(cancellationToken);

            return new JsonResult(suppliers);
        }
    }
}
