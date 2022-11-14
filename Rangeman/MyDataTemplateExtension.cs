namespace Rangeman
{
    using System;
    using System.Xml;
    using Microsoft.Extensions.DependencyInjection;
    using Xamarin.Forms;
    using Xamarin.Forms.Xaml;

    [ContentProperty(nameof(MyDataTemplateExtension.TypeName))]
    public sealed class MyDataTemplateExtension : IMarkupExtension<DataTemplate>
    {
        public string TypeName { get; set; }

        public DataTemplate ProvideValue(IServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            if (!(serviceProvider.GetService(typeof(IXamlTypeResolver)) is IXamlTypeResolver typeResolver))
            {
                throw new ArgumentException("No IXamlTypeResolver in IServiceProvider");
            }

            if (string.IsNullOrEmpty(TypeName))
            {
                IXmlLineInfo li = serviceProvider.GetService(typeof(IXmlLineInfoProvider)) is IXmlLineInfoProvider lip
                    ? lip.XmlLineInfo
                    : new XmlLineInfo();
                throw new XamlParseException("TypeName isn't set.", li);
            }

            if (typeResolver.TryResolve(TypeName, out Type type))
            {
                return new DataTemplate(() => Shell.Current.ServiceProvider().GetRequiredService(type));
            }

            IXmlLineInfo lineInfo = serviceProvider.GetService(typeof(IXmlLineInfoProvider)) is IXmlLineInfoProvider lineInfoProvider
                ? lineInfoProvider.XmlLineInfo
                : new XmlLineInfo();
            throw new XamlParseException($"MyDataTemplateExtension: Could not locate type for {TypeName}.", lineInfo);
        }

        object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
        {
            return (this as IMarkupExtension<DataTemplate>).ProvideValue(serviceProvider);
        }
    }
}