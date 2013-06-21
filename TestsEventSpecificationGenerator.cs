using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace IntelliTrace.Config
{
    using EventSpecification = CollectionPlanTracePointProviderDiagnosticEventSpecification;

    [TestClass]
    public class TestsEventSpecificationGenerator
    {
        [TestMethod]
        public void OverridesResponseRedirect()
        {
            var expected = XElement.Parse(@"<DiagnosticEventSpecification>
        <CategoryId>System.Web</CategoryId>
        <SettingsName _locID='settingsName.HttpResponse.Redirect'>Redirect (HttpResponse)</SettingsName>
        <SettingsDescription _locID='settingsDescription.HttpResponse.Redirect'>Redirect (HttpResponse)</SettingsDescription>
        <Bindings>
          <Binding>
            <ModuleSpecificationId>System.Web</ModuleSpecificationId>
            <TypeName>System.Web.HttpResponse</TypeName>
            <MethodName>Redirect</MethodName>
            <MethodId>System.Web.HttpResponse.Redirect(System.String):System.Void</MethodId>
            <ShortDescription _locID='shortDescription.HttpResponse.Redirect.1'>Redirect</ShortDescription>
            <LongDescription _locID='longDescription.HttpResponse.Redirect.1'>Redirect</LongDescription>
            <DataQueries>
              <DataQuery index='1' maxSize='256' type='String' _locID='dataquery.HttpResponse.Redirect.1.url' _locAttrData='name' name='url' query='' />
            </DataQueries>
          </Binding>
          <Binding>
            <ModuleSpecificationId>System.Web</ModuleSpecificationId>
            <TypeName>System.Web.HttpResponse</TypeName>
            <MethodName>Redirect</MethodName>
            <MethodId>System.Web.HttpResponse.Redirect(System.String,System.Boolean):System.Void</MethodId>
            <ShortDescription _locID='shortDescription.HttpResponse.Redirect.2'>Redirect</ShortDescription>
            <LongDescription _locID='longDescription.HttpResponse.Redirect.2'>Redirect</LongDescription>
            <DataQueries>
              <DataQuery index='1' maxSize='256' type='String' _locID='dataquery.HttpResponse.Redirect.2.url' _locAttrData='name' name='url' query='' />
              <DataQuery index='2' maxSize='0' type='Boolean' _locID='dataquery.HttpResponse.Redirect.2.endResponse' _locAttrData='name' name='endResponse' query='' />
            </DataQueries>
          </Binding>
          <Binding>
            <ModuleSpecificationId>System.Web</ModuleSpecificationId>
            <TypeName>System.Web.HttpResponse</TypeName>
            <MethodName>Redirect</MethodName>
            <MethodId>System.Web.HttpResponse.Redirect(System.String,System.Boolean,System.Boolean):System.Void</MethodId>
            <ShortDescription _locID='shortDescription.HttpResponse.Redirect.3'>Redirect</ShortDescription>
            <LongDescription _locID='longDescription.HttpResponse.Redirect.3'>Redirect</LongDescription>
            <DataQueries>
              <DataQuery index='1' maxSize='256' type='String' _locID='dataquery.HttpResponse.Redirect.3.url' _locAttrData='name' name='url' query='' />
              <DataQuery index='2' maxSize='0' type='Boolean' _locID='dataquery.HttpResponse.Redirect.3.endResponse' _locAttrData='name' name='endResponse' query='' />
              <DataQuery index='3' maxSize='0' type='Boolean' _locID='dataquery.HttpResponse.Redirect.3.permanent' _locAttrData='name' name='permanent' query='' />
            </DataQueries>
          </Binding>
        </Bindings>
      </DiagnosticEventSpecification>");

            var generator = new EventSpecificationGenerator(typeof(System.Web.HttpResponse), "Redirect");

            var result = generator.GetEventSpecification();

            AssertEqual(expected, result);
        }

        [TestMethod]
        public void NoOverridesFileDelete()
        {
            var expected = XElement.Parse(@"<DiagnosticEventSpecification enabled='false'>
        <CategoryId>mscorlib</CategoryId>
        <SettingsName _locID='settingsName.File.Delete'>Delete (File)</SettingsName>
        <SettingsDescription _locID='settingsDescription.File.Delete'>Delete (File)</SettingsDescription>
        <Bindings>
          <Binding>
            <ModuleSpecificationId>mscorlib</ModuleSpecificationId>
            <TypeName>System.IO.File</TypeName>
            <MethodName>Delete</MethodName>
            <MethodId>System.IO.File.Delete(System.String):System.Void</MethodId>
            <ShortDescription _locID='shortDescription.File.Delete'>Delete</ShortDescription>
            <LongDescription _locID='longDescription.File.Delete'>Delete</LongDescription>
            <DataQueries>
              <DataQuery index='1' maxSize='256' type='String' _locID='dataquery.File.Delete.path' _locAttrData='name' name='path' query='' />
            </DataQueries>
          </Binding>
        </Bindings>
      </DiagnosticEventSpecification>");

            var generator = new EventSpecificationGenerator(typeof(System.IO.File), "Delete");

            var result = generator.GetEventSpecification();
            result.enabled = false;

            AssertEqual(expected, result);
        }

        [TestMethod]
        public void OverridesProgrammableDataQueryPageRaisePostBackEvent()
        {
            var expected = XElement.Parse(@"<DiagnosticEventSpecification>
        <CategoryId>System.Web</CategoryId>
        <SettingsName _locID='settingsName.Page.RaisePostBackEvent'>RaisePostBackEvent (Page)</SettingsName>
        <SettingsDescription _locID='settingsDescription.Page.RaisePostBackEvent'>RaisePostBackEvent (Page)</SettingsDescription>
        <Bindings>
            <Binding>
              <ModuleSpecificationId>System.Web</ModuleSpecificationId>
              <TypeName>System.Web.UI.Page</TypeName>
              <MethodName>RaisePostBackEvent</MethodName>
              <MethodId>System.Web.UI.Page.RaisePostBackEvent(System.Web.UI.IPostBackEventHandler,System.String):System.Void</MethodId>
              <ProgrammableDataQuery>
                <ModuleName>Microsoft.VisualStudio.DefaultDataQueries.dll</ModuleName>
                <TypeName>Microsoft.VisualStudio.DataQueries.Webforms.Page.OnPostBackDataQuery</TypeName>
              </ProgrammableDataQuery>
            </Binding>
            <Binding>
                <ModuleSpecificationId>System.Web</ModuleSpecificationId>
                <TypeName>System.Web.UI.Page</TypeName>
                <MethodName>RaisePostBackEvent</MethodName>
                <MethodId>System.Web.UI.Page.RaisePostBackEvent(System.Collections.Specialized.NameValueCollection):System.Void</MethodId>
                <ProgrammableDataQuery>
                  <ModuleName>Microsoft.VisualStudio.DefaultDataQueries.dll</ModuleName>
                  <TypeName>Microsoft.VisualStudio.DataQueries.Webforms.Page.OnPostBackDataQuery</TypeName>
                </ProgrammableDataQuery>
              </Binding>
        </Bindings>
      </DiagnosticEventSpecification>");

            var generator = new EventSpecificationGenerator(typeof(System.Web.UI.Page), "RaisePostBackEvent");

            var result = generator.GetEventSpecification(typeof(Microsoft.VisualStudio.DataQueries.Webforms.Page.OnPostBackDataQuery));

            AssertEqual(expected, result);
        }

        [TestMethod]
        public void NoOverridesProgrammableDataQueryPageOnSaveStateComplete()
        {
            var expected = XElement.Parse(@"<DiagnosticEventSpecification>
        <CategoryId>System.Web</CategoryId>
        <SettingsName _locID='settingsName.Page.OnSaveStateComplete'>OnSaveStateComplete (Page)</SettingsName>
        <SettingsDescription _locID='settingsDescription.Page.OnSaveStateComplete'>OnSaveStateComplete (Page)</SettingsDescription>
        <Bindings>
          <Binding>
            <ModuleSpecificationId>System.Web</ModuleSpecificationId>
            <TypeName>System.Web.UI.Page</TypeName>
            <MethodName>OnSaveStateComplete</MethodName>
            <MethodId>System.Web.UI.Page.OnSaveStateComplete(System.EventArgs):System.Void</MethodId>
            <ProgrammableDataQuery>
              <ModuleName>Microsoft.VisualStudio.DefaultDataQueries.dll</ModuleName>
              <TypeName>Microsoft.VisualStudio.DataQueries.Webforms.Page.OnSaveStateCompleteDataQuery</TypeName>
            </ProgrammableDataQuery>
          </Binding>
        </Bindings>
      </DiagnosticEventSpecification>");

            var generator = new EventSpecificationGenerator(typeof(System.Web.UI.Page), "OnSaveStateComplete");

            var result = generator.GetEventSpecification(typeof(Microsoft.VisualStudio.DataQueries.Webforms.Page.OnSaveStateCompleteDataQuery));

            AssertEqual(expected, result);
        }

        private EventSpecification GetBySettingsName(IList<EventSpecification> list, string name)
        {
            return list.Single(e => e.SettingsName.Value.StartsWith(name));
        }

        private void AssertEqual(XElement expected, EventSpecification actual)
        {
            Assert.IsTrue(Xsi.DeepEqualsWithNormalization(actual.ToXElement(), expected));
        }
    }
}