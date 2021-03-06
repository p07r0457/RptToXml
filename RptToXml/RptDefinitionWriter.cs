﻿using System;
using System.Diagnostics;
using System.Drawing;
using System.Xml;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.Shared;

namespace RptToXml
{
	public class RptDefinitionWriter
	{
		[Flags]
		public enum FormatTypes
		{
			None = 0,
			Border = 2 ^ 0,
			Color = 2 ^ 1,
			Font = 2 ^ 2,
			AreaFormat = 2 ^ 3,
			FieldFormat = 2 ^ 4,
			ObjectFormat = 2 ^ 5,
			SectionFormat = 2 ^ 6,
			All = Border & Color & Font & AreaFormat & FieldFormat & ObjectFormat & SectionFormat
		}

		[Flags]
		public enum ObjectTypes
		{
			None = 0,
			Area = 2 ^ 0,
			Section = 2 ^ 1,
			ReportObject = 2 ^ 2,
			All = Area & Section & ReportObject
		}

		private FormatTypes _showFormatTypes = FormatTypes.AreaFormat | FormatTypes.SectionFormat | FormatTypes.Color;

		private ReportDocument Report { get; set; }

		public FormatTypes ShowFormatTypes
		{
			get { return _showFormatTypes; }
			set { _showFormatTypes = value; }
		}

		public RptDefinitionWriter(string filename)
		{
			Report = new ReportDocument();
			Report.Load(filename, OpenReportMethod.OpenReportByTempCopy);

			Trace.WriteLine("Loaded report");
		}


		public RptDefinitionWriter(ReportDocument value)
		{
			Report = value;
		}

		public void WriteToXml(System.IO.Stream output)
		{
			WriteToXml(XmlWriter.Create(output, new XmlWriterSettings() { Indent = true }));
		}

		public void WriteToXml(string targetXmlPath)
		{
			WriteToXml(XmlWriter.Create(targetXmlPath, new XmlWriterSettings() { Indent = true }));
		}

		public void WriteToXml(XmlWriter writer)
		{
			Trace.WriteLine("Writing to XML");

			writer.WriteStartDocument();
			ProcessReport(Report, writer);
			writer.WriteEndDocument();
			writer.Flush();
		}

		private void ProcessReport(ReportDocument report, XmlWriter writer)
		{
			WriteAndTraceStartElement(writer, "Report");

			writer.WriteAttributeString("Name", report.Name);
			Trace.WriteLine("Writing report " + report.Name);

			if (!report.IsSubreport)
			{
				Trace.WriteLine("Writing header info");

				writer.WriteAttributeString("FileName", report.FileName.Replace("rassdk://", ""));
				writer.WriteAttributeString("HasSavedData", report.HasSavedData.ToString());

				GetSummaryinfo(report, writer);
				GetReportOptions(report, writer);
				GetPrintOptions(report, writer);
				GetSubreports(report, writer);
			}

			GetDatabase(report, writer);
			GetDataDefinition(report, writer);
			GetReportDefinition(report, writer);

			writer.WriteEndElement();
		}

		private static void GetSummaryinfo(ReportDocument report, XmlWriter writer)
		{
			WriteAndTraceStartElement(writer, "Summaryinfo");

			writer.WriteAttributeString("KeywordsinReport", report.SummaryInfo.KeywordsInReport);
			writer.WriteAttributeString("ReportAuthor", report.SummaryInfo.ReportAuthor);
			writer.WriteAttributeString("ReportComments", report.SummaryInfo.ReportComments);
			writer.WriteAttributeString("ReportSubject", report.SummaryInfo.ReportSubject);
			writer.WriteAttributeString("ReportTitle", report.SummaryInfo.ReportTitle);

			writer.WriteEndElement();
		}

		private static void GetReportOptions(ReportDocument report, XmlWriter writer)
		{
			WriteAndTraceStartElement(writer, "ReportOptions");

			writer.WriteAttributeString("EnableSaveDataWithReport", report.ReportOptions.EnableSaveDataWithReport.ToString());
			writer.WriteAttributeString("EnableSavePreviewPicture", report.ReportOptions.EnableSavePreviewPicture.ToString());
			writer.WriteAttributeString("EnableSaveSummariesWithReport", report.ReportOptions.EnableSaveSummariesWithReport.ToString());
			writer.WriteAttributeString("EnableUseDummyData", report.ReportOptions.EnableUseDummyData.ToString());
			writer.WriteAttributeString("initialDataContext", report.ReportOptions.InitialDataContext);
			writer.WriteAttributeString("initialReportPartName", report.ReportOptions.InitialDataContext);

			writer.WriteEndElement();
		}

		private static void GetPrintOptions(ReportDocument report, XmlWriter writer)
		{
			WriteAndTraceStartElement(writer, "PrintOptions");

			writer.WriteAttributeString("PageContentHeight", report.PrintOptions.PageContentHeight.ToString());
			writer.WriteAttributeString("PageContentWidth", report.PrintOptions.PageContentWidth.ToString());
			writer.WriteAttributeString("PaperOrientation", report.PrintOptions.PaperOrientation.ToString());
			writer.WriteAttributeString("PaperSize", report.PrintOptions.PaperSize.ToString());
			writer.WriteAttributeString("PaperSource", report.PrintOptions.PaperSource.ToString());
			writer.WriteAttributeString("PrinterDuplex", report.PrintOptions.PrinterDuplex.ToString());
			writer.WriteAttributeString("PrinterName", report.PrintOptions.PrinterName);

			WriteAndTraceStartElement(writer, "PageMargins");

			writer.WriteAttributeString("bottomMargin", report.PrintOptions.PageMargins.bottomMargin.ToString());
			writer.WriteAttributeString("leftMargin", report.PrintOptions.PageMargins.leftMargin.ToString());
			writer.WriteAttributeString("rightMargin", report.PrintOptions.PageMargins.rightMargin.ToString());
			writer.WriteAttributeString("topMargin", report.PrintOptions.PageMargins.topMargin.ToString());

			writer.WriteEndElement();

			writer.WriteEndElement();
		}

		private void GetSubreports(ReportDocument report, XmlWriter writer)
		{
			WriteAndTraceStartElement(writer, "SubReports");

			foreach (ReportDocument subreport in report.Subreports)
			{
				ProcessReport(subreport, writer);
			}

			writer.WriteEndElement();
		}

		private void GetDatabase(ReportDocument report, XmlWriter writer)
		{
			WriteAndTraceStartElement(writer, "Database");

			GetTableLinks(report, writer);
			GetTables(report, writer);

			writer.WriteEndElement();

		}

		private static void GetTableLinks(ReportDocument report, XmlWriter writer)
		{
			WriteAndTraceStartElement(writer, "TableLinks");

			foreach (TableLink tl in report.Database.Links)
			{
				WriteAndTraceStartElement(writer, "TableLink");

				writer.WriteAttributeString("JoinType", tl.JoinType.ToString());


				WriteAndTraceStartElement(writer, "SourceFields");

				foreach (FieldDefinition fd in tl.SourceFields)
					GetFieldDefinition(fd, writer);

				writer.WriteEndElement();





				WriteAndTraceStartElement(writer, "DestinationFields");

				foreach (FieldDefinition fd in tl.DestinationFields)
					GetFieldDefinition(fd, writer);

				writer.WriteEndElement();

				writer.WriteEndElement();
			}

			writer.WriteEndElement();

		}

		private void GetTables(ReportDocument report, XmlWriter writer)
		{
			WriteAndTraceStartElement(writer, "Tables");

			foreach (Table T in report.Database.Tables)
				GetTable(T, writer);

			writer.WriteEndElement();

		}

		private static void GetTable(Table table, XmlWriter writer)
		{
			WriteAndTraceStartElement(writer, "Table");

			writer.WriteAttributeString("Location", table.Location);
			writer.WriteAttributeString("Name", table.Name);

			GetLogoninfo(table.LogOnInfo, writer);

			WriteAndTraceStartElement(writer, "Fields");

			foreach (FieldDefinition fd in table.Fields)
				GetFieldDefinition(fd, writer);

			writer.WriteEndElement();

			writer.WriteEndElement();

		}

		private static void GetLogoninfo(TableLogOnInfo li, XmlWriter writer)
		{
			WriteAndTraceStartElement(writer, "Logoninfo");

			writer.WriteAttributeString("ReportName", li.ReportName);
			writer.WriteAttributeString("TableName", li.TableName);

			GetConnectioInfo(li.ConnectionInfo, writer);

			writer.WriteEndElement();

		}

		private static void GetConnectioInfo(ConnectionInfo ci, XmlWriter writer)
		{
			WriteAndTraceStartElement(writer, "ConnectioInfo");

			writer.WriteAttributeString("AllowCustomConnection", ci.AllowCustomConnection.ToString());

			writer.WriteAttributeString("DatabaseName", ci.DatabaseName);
			writer.WriteAttributeString("integratedSecurity", ci.IntegratedSecurity.ToString());
			writer.WriteAttributeString("Password", ci.Password);
			writer.WriteAttributeString("ServerName", ci.ServerName);
			writer.WriteAttributeString("Type", ci.Type.ToString());
			writer.WriteAttributeString("UserID", ci.UserID);

			writer.WriteEndElement();

		}

		private static void GetFieldDefinition(FieldDefinition fd, XmlWriter writer)
		{
			WriteAndTraceStartElement(writer, "Field");

			writer.WriteAttributeString("FormulaName", fd.FormulaName);
			writer.WriteAttributeString("Kind", fd.Kind.ToString());
			writer.WriteAttributeString("Name", fd.Name);
			writer.WriteAttributeString("NumberOfBytes", fd.NumberOfBytes.ToString());
			writer.WriteAttributeString("ValueType", fd.ValueType.ToString());

			writer.WriteEndElement();

		}

		private static void GetDataDefinition(ReportDocument report, XmlWriter writer)
		{
			WriteAndTraceStartElement(writer, "DataDefinition");

			writer.WriteElementString("GroupSelectionFormula", report.DataDefinition.GroupSelectionFormula);

			writer.WriteElementString("RecordSelectionFormula", report.DataDefinition.RecordSelectionFormula);

			WriteAndTraceStartElement(writer, "Groups");
			foreach (Group group in report.DataDefinition.Groups)
			{
				WriteAndTraceStartElement(writer, "Group");
				writer.WriteAttributeString("ConditionField", group.ConditionField.FormulaName);
				writer.WriteEndElement();
			}

			writer.WriteEndElement();

			WriteAndTraceStartElement(writer, "SortFields");
			foreach (SortField sortField in report.DataDefinition.SortFields)
			{
				WriteAndTraceStartElement(writer, "SortField");

				writer.WriteAttributeString("Field", sortField.Field.FormulaName);
				try
				{
					string sortDirection = sortField.SortDirection.ToString();
					writer.WriteAttributeString("SortDirection", sortDirection);
				}
				catch (NotSupportedException)
				{}
				writer.WriteAttributeString("SortType", sortField.SortType.ToString());

				writer.WriteEndElement();
			}
			writer.WriteEndElement();

			WriteAndTraceStartElement(writer, "FormulaFieldDefinitions");
			foreach (var field in report.DataDefinition.FormulaFields)
				GetFieldObject(field, writer);

			writer.WriteEndElement();

			WriteAndTraceStartElement(writer, "GroupNameFieldDefinitions");
			foreach (var field in report.DataDefinition.GroupNameFields)
				GetFieldObject(field, writer);

			writer.WriteEndElement();

			WriteAndTraceStartElement(writer, "ParameterFieldDefinitions");
			foreach (var field in report.DataDefinition.ParameterFields)
				GetFieldObject(field, writer);

			writer.WriteEndElement();

			WriteAndTraceStartElement(writer, "RunningTotalFieldDefinitions");
			foreach (var field in report.DataDefinition.RunningTotalFields)
				GetFieldObject(field, writer);

			writer.WriteEndElement();

			WriteAndTraceStartElement(writer, "SQLExpressionFields");
			foreach (var field in report.DataDefinition.SQLExpressionFields)
				GetFieldObject(field, writer);

			writer.WriteEndElement();

			WriteAndTraceStartElement(writer, "SummaryFields");
			foreach (var field in report.DataDefinition.SummaryFields)
				GetFieldObject(field, writer);

			writer.WriteEndElement();


			writer.WriteEndElement();

		}

		private static void GetFieldObject(Object fo, XmlWriter writer)
		{
			if (fo is DatabaseFieldDefinition)
			{
				var df = (DatabaseFieldDefinition)fo;

				WriteAndTraceStartElement(writer, "DatabaseFieldDefinition");

				writer.WriteAttributeString("FormulaName", df.FormulaName);
				writer.WriteAttributeString("Kind", df.Kind.ToString());
				writer.WriteAttributeString("Name", df.Name);
				writer.WriteAttributeString("NumberOfBytes", df.NumberOfBytes.ToString());
				writer.WriteAttributeString("TableName", df.TableName);
				writer.WriteAttributeString("ValueType", df.ValueType.ToString());

				writer.WriteEndElement();
			}
			else if (fo is FormulaFieldDefinition)
			{
				var ff = (FormulaFieldDefinition)fo;

				WriteAndTraceStartElement(writer, "FormulaFieldDefinition");

				writer.WriteAttributeString("FormulaName", ff.FormulaName);
				writer.WriteAttributeString("Kind", ff.Kind.ToString());
				writer.WriteAttributeString("Name", ff.Name);
				writer.WriteAttributeString("NumberOfBytes", ff.NumberOfBytes.ToString());
				writer.WriteAttributeString("ValueType", ff.ValueType.ToString());
				writer.WriteString(ff.Text);


				writer.WriteEndElement();
			}
			else if (fo is GroupNameFieldDefinition)
			{
				var gnf = (GroupNameFieldDefinition)fo;

				WriteAndTraceStartElement(writer, "GroupNameFieldDefinition");

				writer.WriteAttributeString("FormulaName", gnf.FormulaName);
				writer.WriteAttributeString("Group", gnf.Group.ToString());
				writer.WriteAttributeString("GroupNameFieldName", gnf.GroupNameFieldName);
				writer.WriteAttributeString("Kind", gnf.Kind.ToString());
				writer.WriteAttributeString("Name", gnf.Name);
				writer.WriteAttributeString("NumberOfBytes", gnf.NumberOfBytes.ToString());
				writer.WriteAttributeString("ValueType", gnf.ValueType.ToString());

				writer.WriteEndElement();
			}
			else if (fo is ParameterFieldDefinition)
			{
				var pf = (ParameterFieldDefinition)fo;

				WriteAndTraceStartElement(writer, "ParameterFieldDefinition");

				writer.WriteAttributeString("EditMask", pf.EditMask);
				writer.WriteAttributeString("EnableAllowEditingDefaultValue", pf.EnableAllowEditingDefaultValue.ToString());
				writer.WriteAttributeString("EnableAllowMultipleValue", pf.EnableAllowMultipleValue.ToString());
				writer.WriteAttributeString("EnableNullValue", pf.EnableNullValue.ToString());
				writer.WriteAttributeString("FormulaName", pf.FormulaName);
				writer.WriteAttributeString("HasCurrentValue", pf.HasCurrentValue.ToString());
				writer.WriteAttributeString("Kind", pf.Kind.ToString());
				//writer.WriteAttributeString("MaximumValue", (string) pf.MaximumValue);
				//writer.WriteAttributeString("MinimumValue", (string) pf.MinimumValue);
				writer.WriteAttributeString("Name", pf.Name);
				writer.WriteAttributeString("NumberOfBytes", pf.NumberOfBytes.ToString());
				writer.WriteAttributeString("ParameterFieldName", pf.ParameterFieldName);
				writer.WriteAttributeString("ParameterFieldUsage", pf.ParameterFieldUsage2.ToString());
				writer.WriteAttributeString("ParameterType", pf.ParameterType.ToString());
				writer.WriteAttributeString("ParameterValueKind", pf.ParameterValueKind.ToString());
				writer.WriteAttributeString("PromptText", pf.PromptText);
				writer.WriteAttributeString("ReportName", pf.ReportName);
				writer.WriteAttributeString("ValueType", pf.ValueType.ToString());

				writer.WriteEndElement();
			}
			else if (fo is RunningTotalFieldDefinition)
			{
				var rtf = (RunningTotalFieldDefinition)fo;

				WriteAndTraceStartElement(writer, "RunningTotalFieldDefinition");
				//writer.WriteAttributeString("EvaluationConditionType", rtf.EvaluationCondition);
				writer.WriteAttributeString("EvaluationConditionType", rtf.EvaluationConditionType.ToString());
				writer.WriteAttributeString("FormulaName", rtf.FormulaName);
				if (rtf.Group != null) writer.WriteAttributeString("Group", rtf.Group.ToString());
				writer.WriteAttributeString("Kind", rtf.Kind.ToString());
				writer.WriteAttributeString("Name", rtf.Name);
				writer.WriteAttributeString("NumberOfBytes", rtf.NumberOfBytes.ToString());
				writer.WriteAttributeString("Operation", rtf.Operation.ToString());
				writer.WriteAttributeString("OperationParameter", rtf.OperationParameter.ToString());
				//writer.WriteAttributeString("ResetCondition", rtf.ResetCondition);
				writer.WriteAttributeString("ResetConditionType", rtf.ResetConditionType.ToString());
				
				if (rtf.SecondarySummarizedField != null)
					writer.WriteAttributeString("SecondarySummarizedField", rtf.SecondarySummarizedField.FormulaName);

				writer.WriteAttributeString("SummarizedField", rtf.SummarizedField.FormulaName);
				writer.WriteAttributeString("ValueType", rtf.ValueType.ToString());

				writer.WriteEndElement();
			}
			else if (fo is SpecialVarFieldDefinition)
			{
				WriteAndTraceStartElement(writer, "SpecialVarFieldDefinition");
				var svf = (SpecialVarFieldDefinition)fo;
				writer.WriteAttributeString("FormulaName", svf.FormulaName);
				writer.WriteAttributeString("Kind", svf.Kind.ToString());
				writer.WriteAttributeString("Name", svf.Name);
				writer.WriteAttributeString("NumberOfBytes", svf.NumberOfBytes.ToString());
				writer.WriteAttributeString("SpecialVarType", svf.SpecialVarType.ToString());
				writer.WriteAttributeString("ValueType", svf.ValueType.ToString());

				writer.WriteEndElement();
			}
			else if (fo is SQLExpressionFieldDefinition)
			{
				WriteAndTraceStartElement(writer, "SQLExpressionFieldDefinition");
				var sef = (SQLExpressionFieldDefinition)fo;

				writer.WriteAttributeString("FormulaName", sef.FormulaName);
				writer.WriteAttributeString("Kind", sef.Kind.ToString());
				writer.WriteAttributeString("Name", sef.Name);
				writer.WriteAttributeString("NumberOfBytes", sef.NumberOfBytes.ToString());
				writer.WriteAttributeString("Text", sef.Text);
				writer.WriteAttributeString("ValueType", sef.ValueType.ToString());

				writer.WriteEndElement();
			}
			else if (fo is SummaryFieldDefinition)
			{
				WriteAndTraceStartElement(writer, "SummaryFieldDefinition");

				var sf = (SummaryFieldDefinition)fo;

				writer.WriteAttributeString("FormulaName", sf.FormulaName);
				
				if (sf.Group != null)
					writer.WriteAttributeString("Group", sf.Group.ToString());

				writer.WriteAttributeString("Kind", sf.Kind.ToString());
				writer.WriteAttributeString("Name", sf.Name);
				writer.WriteAttributeString("NumberOfBytes", sf.NumberOfBytes.ToString());
				writer.WriteAttributeString("Operation", sf.Operation.ToString());
				writer.WriteAttributeString("OperationParameter", sf.OperationParameter.ToString());
				if (sf.SecondarySummarizedField != null) writer.WriteAttributeString("SecondarySummarizedField", sf.SecondarySummarizedField.ToString());
				writer.WriteAttributeString("SummarizedField", sf.SummarizedField.ToString());
				writer.WriteAttributeString("ValueType", sf.ValueType.ToString());

				writer.WriteEndElement();
			}
		}

		private static void GetAreaFormat(AreaFormat areaFormat, XmlWriter writer)
		{
			WriteAndTraceStartElement(writer, "AreaFormat");

			writer.WriteAttributeString("EnableHideForDrillDown", areaFormat.EnableHideForDrillDown.ToString());
			writer.WriteAttributeString("EnableKeepTogether", areaFormat.EnableKeepTogether.ToString());
			writer.WriteAttributeString("EnableNewPageAfter", areaFormat.EnableNewPageAfter.ToString());
			writer.WriteAttributeString("EnableNewPageBefore", areaFormat.EnableNewPageBefore.ToString());
			writer.WriteAttributeString("EnablePrintAtBottomOfPage", areaFormat.EnablePrintAtBottomOfPage.ToString());
			writer.WriteAttributeString("EnableResetPageNumberAfter", areaFormat.EnableResetPageNumberAfter.ToString());
			writer.WriteAttributeString("EnableSuppress", areaFormat.EnableSuppress.ToString());

			writer.WriteEndElement();

		}

		private void GetBorderFormat(Border border, XmlWriter writer)
		{
			WriteAndTraceStartElement(writer, "Border");

			writer.WriteAttributeString("BottomLineStyle", border.BottomLineStyle.ToString());
			writer.WriteAttributeString("HasDropShadow", border.HasDropShadow.ToString());
			writer.WriteAttributeString("LeftLineStyle", border.LeftLineStyle.ToString());
			writer.WriteAttributeString("RightLineStyle", border.RightLineStyle.ToString());
			writer.WriteAttributeString("TopLineStyle", border.TopLineStyle.ToString());
			if ((ShowFormatTypes & FormatTypes.Color) == FormatTypes.Color)
				GetColorFormat(border.BackgroundColor, writer, "BackgroundColor");
			if ((ShowFormatTypes & FormatTypes.Color) == FormatTypes.Color)
				GetColorFormat(border.BorderColor, writer, "BorderColor");

			writer.WriteEndElement();

		}

		private static void GetColorFormat(Color color, XmlWriter writer, String elementName = "Color")
		{
			WriteAndTraceStartElement(writer, "Color");

			writer.WriteAttributeString("Name", color.Name);
			writer.WriteAttributeString("A", color.A.ToString());
			writer.WriteAttributeString("R", color.R.ToString());
			writer.WriteAttributeString("G", color.G.ToString());
			writer.WriteAttributeString("B", color.B.ToString());

			writer.WriteEndElement();

		}

		private static void GetFontFormat(Font font, XmlWriter writer)
		{
			WriteAndTraceStartElement(writer, "Font");

			writer.WriteAttributeString("Bold", font.Bold.ToString());
			writer.WriteAttributeString("FontFamily", font.FontFamily.Name);
			writer.WriteAttributeString("GdiCharSet", font.GdiCharSet.ToString());
			writer.WriteAttributeString("GdiVerticalFont", font.GdiVerticalFont.ToString());
			writer.WriteAttributeString("Height", font.Height.ToString());
			writer.WriteAttributeString("IsSystemFont", font.IsSystemFont.ToString());
			writer.WriteAttributeString("Italic", font.Italic.ToString());
			writer.WriteAttributeString("Name", font.Name);
			writer.WriteAttributeString("OriginalFontName", font.OriginalFontName);
			writer.WriteAttributeString("Size", font.Size.ToString());
			writer.WriteAttributeString("SizeinPoints", font.SizeInPoints.ToString());
			writer.WriteAttributeString("Strikeout", font.Strikeout.ToString());
			writer.WriteAttributeString("Style", font.Style.ToString());
			writer.WriteAttributeString("SystemFontName", font.SystemFontName);
			writer.WriteAttributeString("Underline", font.Underline.ToString());
			writer.WriteAttributeString("Unit", font.Unit.ToString());

			writer.WriteEndElement();

		}

		private static void GetObjectFormat(ObjectFormat objectFormat, XmlWriter writer)
		{
			WriteAndTraceStartElement(writer, "ObjectFormat");

			writer.WriteAttributeString("CssClass", objectFormat.CssClass);
			writer.WriteAttributeString("EnableCanGrow", objectFormat.EnableCanGrow.ToString());
			writer.WriteAttributeString("EnableCloseAtPageBreak", objectFormat.EnableCloseAtPageBreak.ToString());
			writer.WriteAttributeString("EnableKeepTogether", objectFormat.EnableKeepTogether.ToString());
			writer.WriteAttributeString("EnableSuppress", objectFormat.EnableSuppress.ToString());
			writer.WriteAttributeString("HorizontalAlignment", objectFormat.HorizontalAlignment.ToString());

			writer.WriteEndElement();

		}

		private void GetSectionFormat(SectionFormat sectionFormat, XmlWriter writer)
		{
			WriteAndTraceStartElement(writer, "SectionFormat");

			writer.WriteAttributeString("CssClass", sectionFormat.CssClass);
			writer.WriteAttributeString("EnableKeepTogether", sectionFormat.EnableKeepTogether.ToString());
			writer.WriteAttributeString("EnableNewPageAfter", sectionFormat.EnableNewPageAfter.ToString());
			writer.WriteAttributeString("EnableNewPageBefore", sectionFormat.EnableNewPageBefore.ToString());
			writer.WriteAttributeString("EnablePrintAtBottomOfPage", sectionFormat.EnablePrintAtBottomOfPage.ToString());
			writer.WriteAttributeString("EnableResetPageNumberAfter", sectionFormat.EnableResetPageNumberAfter.ToString());
			writer.WriteAttributeString("EnableSuppress", sectionFormat.EnableSuppress.ToString());
			writer.WriteAttributeString("EnableSuppressIfBlank", sectionFormat.EnableSuppressIfBlank.ToString());
			writer.WriteAttributeString("EnableUnderlaySection", sectionFormat.EnableUnderlaySection.ToString());
			if ((ShowFormatTypes & FormatTypes.Color) == FormatTypes.Color)
				GetColorFormat(sectionFormat.BackgroundColor, writer, "BackgroundColor");

			writer.WriteEndElement();

		}

		private void GetReportDefinition(ReportDocument report, XmlWriter writer)
		{
			WriteAndTraceStartElement(writer, "ReportDefinition");

			GetAreas(report.ReportDefinition, writer);

			writer.WriteEndElement();

		}

		private void GetAreas(ReportDefinition reportDefinition, XmlWriter writer)
		{
			WriteAndTraceStartElement(writer, "Areas");

			foreach (Area area in reportDefinition.Areas)
			{
				WriteAndTraceStartElement(writer, "Area");

				writer.WriteAttributeString("Kind", area.Kind.ToString());
				writer.WriteAttributeString("Name", area.Name);

				if ((ShowFormatTypes & FormatTypes.AreaFormat) == FormatTypes.AreaFormat)
					GetAreaFormat(area.AreaFormat, writer);

				GetSections(area, writer);

				writer.WriteEndElement();
			}

			writer.WriteEndElement();
		}

		private void GetSections(Area area, XmlWriter writer)
		{
			WriteAndTraceStartElement(writer, "Sections");

			foreach (Section section in area.Sections)
			{
				WriteAndTraceStartElement(writer, "Section");


				writer.WriteAttributeString("Height", section.Height.ToString());
				writer.WriteAttributeString("Kind", section.Kind.ToString());
				writer.WriteAttributeString("Name", section.Name);

				if ((ShowFormatTypes & FormatTypes.SectionFormat) == FormatTypes.SectionFormat)
					GetSectionFormat(section.SectionFormat, writer);

				GetReportObjects(section, writer);

				writer.WriteEndElement();
			}

			writer.WriteEndElement();

		}

		private void GetReportObjects(Section section, XmlWriter writer)
		{
			WriteAndTraceStartElement(writer, "ReportObjects");

			foreach (ReportObject reportObject in section.ReportObjects)
			{
				WriteAndTraceStartElement(writer, reportObject.GetType().Name);

				writer.WriteAttributeString("Name", reportObject.Name);
				writer.WriteAttributeString("Kind", reportObject.Kind.ToString());

				writer.WriteAttributeString("Top", reportObject.Top.ToString());
				writer.WriteAttributeString("Left", reportObject.Left.ToString());
				writer.WriteAttributeString("Width", reportObject.Width.ToString());
				writer.WriteAttributeString("Height", reportObject.Height.ToString());

				if (reportObject is BoxObject)
				{
					var bo = (BoxObject)reportObject;
					writer.WriteAttributeString("Bottom", bo.Bottom.ToString());
					writer.WriteAttributeString("EnableExtendToBottomOfSection", bo.EnableExtendToBottomOfSection.ToString());
					writer.WriteAttributeString("EndSectionName", bo.EndSectionName);
					writer.WriteAttributeString("LineStyle", bo.LineStyle.ToString());
					writer.WriteAttributeString("LineThickness", bo.LineThickness.ToString());
					writer.WriteAttributeString("Right", bo.Right.ToString());
					if ((ShowFormatTypes & FormatTypes.Color) == FormatTypes.Color)
						GetColorFormat(bo.LineColor, writer, "LineColor");
				}
				else if (reportObject is DrawingObject)
				{
					var dobj = (DrawingObject)reportObject;
					writer.WriteAttributeString("Bottom", dobj.Bottom.ToString());
					writer.WriteAttributeString("EnableExtendToBottomOfSection", dobj.EnableExtendToBottomOfSection.ToString());
					writer.WriteAttributeString("EndSectionName", dobj.EndSectionName);
					writer.WriteAttributeString("LineStyle", dobj.LineStyle.ToString());
					writer.WriteAttributeString("LineThickness", dobj.LineThickness.ToString());
					writer.WriteAttributeString("Right", dobj.Right.ToString());
					if ((ShowFormatTypes & FormatTypes.Color) == FormatTypes.Color)
						GetColorFormat(dobj.LineColor, writer, "LineColor");
				}
				else if (reportObject is FieldHeadingObject)
				{
					var fh = (FieldHeadingObject)reportObject;
					writer.WriteAttributeString("FieldObjectName", fh.FieldObjectName);
					writer.WriteElementString("Text", fh.Text);
				}
				else if (reportObject is FieldObject)
				{
					var fo = (FieldObject)reportObject;

					writer.WriteAttributeString("DataSource", fo.DataSource.FormulaName);

					if ((ShowFormatTypes & FormatTypes.Color) == FormatTypes.Color)
						GetColorFormat(fo.Color, writer);

					if ((ShowFormatTypes & FormatTypes.Font) == FormatTypes.Font)
						GetFontFormat(fo.Font, writer);
				}
				else if (reportObject is LineObject)
				{
					var lo = (LineObject)reportObject;
					writer.WriteAttributeString("Bottom", lo.Bottom.ToString());
					writer.WriteAttributeString("EnableExtendToBottomOfSection", lo.EnableExtendToBottomOfSection.ToString());
					writer.WriteAttributeString("EndSectionName", lo.EndSectionName);
					writer.WriteAttributeString("LineStyle", lo.LineStyle.ToString());
					writer.WriteAttributeString("LineThickness", lo.LineThickness.ToString());
					writer.WriteAttributeString("Right", lo.Right.ToString());
					if ((ShowFormatTypes & FormatTypes.Color) == FormatTypes.Color)
						GetColorFormat(lo.LineColor, writer, "LineColor");
				}
				else if (reportObject is TextObject)
				{
					var tobj = (TextObject)reportObject;
					writer.WriteElementString("Text", tobj.Text);

					if ((ShowFormatTypes & FormatTypes.Color) == FormatTypes.Color)
						GetColorFormat(tobj.Color, writer);
					if ((ShowFormatTypes & FormatTypes.Font) == FormatTypes.Font)
						GetFontFormat(tobj.Font, writer);
				}

				if ((ShowFormatTypes & FormatTypes.Border) == FormatTypes.Border)
					GetBorderFormat(reportObject.Border, writer);

				if ((ShowFormatTypes & FormatTypes.ObjectFormat) == FormatTypes.ObjectFormat)
					GetObjectFormat(reportObject.ObjectFormat, writer);

				writer.WriteEndElement();

			}

			writer.WriteEndElement();

		}

		private static void WriteAndTraceStartElement(XmlWriter writer, string elementName)
		{
			Trace.WriteLine("  " + elementName);
			writer.WriteStartElement(elementName);
		}
	}
}