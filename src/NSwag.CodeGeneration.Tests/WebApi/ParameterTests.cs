﻿using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NJsonSchema;
using NSwag.CodeGeneration.SwaggerGenerators.WebApi;

namespace NSwag.CodeGeneration.Tests.WebApi
{
    [TestClass]
    public class ParameterTests
    {
        public interface IFormFile
        {
        }

        public interface IActionResult
        {
        }
        
        public class FromUriParameterController : ApiController
        {
            public class ComplexClass
            {
                public string Foo { get; set; }

                public string Bar { get; set; }
            }

            [HttpPost, Route("upload")]
            public IActionResult Upload([FromUri]ComplexClass parameters)
            {
                throw new NotImplementedException();
            }
        }

        [TestMethod]
        public async Task When_parameter_is_from_uri_then_two_params_are_generated()
        {
            //// Arrange
            var generator = new WebApiToSwaggerGenerator(new WebApiToSwaggerGeneratorSettings());

            //// Act
            var document = await generator.GenerateForControllerAsync(typeof(FromUriParameterController));

            //// Assert
            var operation = document.Paths["/upload"][SwaggerOperationMethod.Post];

            Assert.AreEqual(JsonObjectType.String, operation.ActualParameters.Single(p => p.Name == "Foo").Type);
            Assert.AreEqual(JsonObjectType.String, operation.ActualParameters.Single(p => p.Name == "Bar").Type);

            Assert.IsTrue(operation.ActualParameters.Any(p => p.Name == "Foo"));
            Assert.IsTrue(operation.ActualParameters.Any(p => p.Name == "Bar"));

            Assert.IsNull(operation.Consumes);
        }

        public class ConstrainedRoutePathController : ApiController
        {
            [Route("{id:long:min(1)}")]
            public object Get(long id)
            {
                return null;
            }
        }

        [TestMethod]
        public async Task When_web_api_path_has_constraints_then_they_are_removed_in_the_swagger_spec()
        {
            //// Arrange
            var generator = new WebApiToSwaggerGenerator(new WebApiToSwaggerGeneratorSettings());

            //// Act
            var document = await generator.GenerateForControllerAsync(typeof(ConstrainedRoutePathController));

            //// Assert
            var path = document.Paths.First().Key;

            Assert.AreEqual("/{id}", path);
        }

        [Route("account/{action}/{id?}")]
        public class AccountController : ApiController
        {
            [HttpGet]
            public string Get()
            {
                return null;
            }

            [HttpGet]
            public string GetAll()
            {
                return null;
            }

            [HttpPost]
            public async Task<IHttpActionResult> Post([FromBody] object model)
            {
                return null;
            }

            [HttpPost]
            public async Task<IHttpActionResult> Verify([FromBody] object model)
            {
                return null;
            }

            [HttpPost]
            public async Task<IHttpActionResult> Confirm([FromBody] object model)
            {
                return null;
            }
        }

        [TestMethod]
        public async Task When_class_has_RouteAttribute_with_placeholders_then_they_are_correctly_replaced()
        {
            //// Arrange
            var generator = new WebApiToSwaggerGenerator(new WebApiToSwaggerGeneratorSettings());

            //// Act
            var document = await generator.GenerateForControllerAsync(typeof(AccountController));

            //// Assert
            Assert.IsTrue(document.Paths.ContainsKey("/account/Get"));
            Assert.IsTrue(document.Paths.ContainsKey("/account/GetAll"));
            Assert.IsTrue(document.Paths.ContainsKey("/account/Post"));
            Assert.IsTrue(document.Paths.ContainsKey("/account/Verify"));
            Assert.IsTrue(document.Paths.ContainsKey("/account/Confirm"));
        }
    }
}
