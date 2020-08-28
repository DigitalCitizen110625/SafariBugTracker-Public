using Microsoft.AspNetCore.Mvc.ModelBinding;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace SafariBugTracker.IssueAPI.Helpers
{
    /// <summary>
    /// Contains methods for binding a collection of items into a model
    /// </summary>
    public class ArrayModelBinder : IModelBinder
    {

        /// <summary>
        /// Custom implementation of the IModelBinder interface. Allows the binding of incoming arguments to custom models
        /// </summary>
        /// <param name="bindingContext"> Contains meta data about the model, including it's values and type</param>
        /// <returns>Task indicating the result of the operation</returns>
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            //Our binder works only on enumerable types
            if (!bindingContext.ModelMetadata.IsEnumerableType)
            {
                bindingContext.Result = ModelBindingResult.Failed();
                return Task.CompletedTask;
            }

            //Get the inputted value through the value provider
            var value = bindingContext.ValueProvider
                .GetValue(bindingContext.ModelName).ToString();

            //If that value is null or whitespace, we return null
            if (string.IsNullOrWhiteSpace(value))
            {
                bindingContext.Result = ModelBindingResult.Success(null);
                return Task.CompletedTask;
            }

            //The value isn't null or whitespace, 
            //and the type of the model is enumerable. 
            //Get the enumerable's type, and a converter 
            var elementType = bindingContext.ModelType.GetTypeInfo().GenericTypeArguments[0];
            var converter = TypeDescriptor.GetConverter(elementType);

            //Convert each item in the value list to the enumerable type
            var values = value
                .Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries)
                .Select(x => converter.ConvertFromString(x.Trim()))
                .ToArray();

            //Create an array of that type, and set it as the Model value 
            var typedValues = Array.CreateInstance(elementType, values.Length);
            values.CopyTo(typedValues, 0);
            bindingContext.Model = typedValues;

            //Return a successful result, passing in the Model 
            bindingContext.Result = ModelBindingResult.Success(bindingContext.Model);
            return Task.CompletedTask;
        }
    }//class
}//namespace