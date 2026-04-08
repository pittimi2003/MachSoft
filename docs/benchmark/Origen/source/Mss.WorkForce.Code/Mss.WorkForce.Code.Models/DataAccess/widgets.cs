using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mss.WorkForce.Code.Models.DataAccess
{
    public class widgets
    {
public static string view1 = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Dashboard>
  <Title Text=""VIEW1"" />
  <DataSources>
    <SqlDataSource Name=""combinedplanningview"" ComponentName=""sqlDataSource1"">
      <Connection Name=""Metrics"" FromAppConfig=""true"" />
      <Query Type=""CustomSqlQuery"" Name=""combinedplanningview"">
        <Sql>
          SELECT *
          FROM public.planningresourcesview_p('{PlanningId}'::uuid, '{WarehouseId}'::uuid, '{UtcOffset}', '{HourFormat}')
        </Sql>
      </Query>
      <ConnectionOptions CloseConnection=""true"" />
      <CalculatedFields>
        <CalculatedField Name=""InitDate_min"" Expression=""aggr(Min([InitDate]),[ProcessId],[ProcessName], GetDate([When]) )"" DataType=""Auto"" DataMember=""combinedplanningview"" />
        <CalculatedField Name=""EndDate_Max"" Expression=""aggr(Max([EndDate]),[ProcessId],[ProcessName], GetDate([When]))"" DataType=""Auto"" DataMember=""combinedplanningview"" />
        <CalculatedField Name=""Today"" Expression=""Today()"" DataType=""Auto"" DataMember=""combinedplanningview"" />
        <CalculatedField Name=""ResourcesNotUsed"" Expression=""[AvailableResources_Max]- [Resources_Count]"" DataType=""Double"" DataMember=""combinedplanningview"" />
        <CalculatedField Name=""AvailableResources_Max"" Expression=""Max([AvailableResources])"" DataType=""Double"" DataMember=""combinedplanningview"" />
        <CalculatedField Name=""Resources_Count"" Expression=""CountDistinct([ResourceId])"" DataType=""Double"" DataMember=""combinedplanningview"" />
      </CalculatedFields>
    </SqlDataSource>
<SqlDataSource Name=""processinterleavingview"" ComponentName=""sqlDataSource2"">
  <Connection Name=""Metrics"" FromAppConfig=""true"" />
  <Query Type=""CustomSqlQuery"" Name=""processinterleavingview"">
    <Sql>
      SELECT
        ""When"",
        ""InboundSec"",
        ""OutboundSec"",
        ""InterleavingSec"",
        ""NoWorkingSec""
      FROM public.processinterleavingview_p('{PlanningId}'::uuid, '{UtcOffset}', '{HourFormat}')
    </Sql>
  </Query>
  <ConnectionOptions CloseConnection=""true"" />
  <CalculatedFields>
    <CalculatedField Name=""Today""
                     Expression=""Today()""
                     DataType=""Auto""
                     DataMember=""processinterleavingview"" />
  </CalculatedFields>
</SqlDataSource>
    <SqlDataSource Name=""areaview"" ComponentName=""sqlDataSource3"">
      <Connection Name=""Metrics"" FromAppConfig=""true"" />
      <Query Type=""CustomSqlQuery"" Name=""areaview"">
        <Sql>
        SELECT
          ""AreaName"",
          ""When"",
          ""AreasPercUt"" AS ""areaspercut""
        FROM public.areaview_p('{PlanningId}'::uuid, '{WarehouseId}'::uuid, '{UtcOffset}', '{HourFormat}');
        </Sql>
      </Query>
      <ConnectionOptions CloseConnection=""true"" />
      <CalculatedFields>
        <CalculatedField Name=""NonUtilizationTime""
                         Expression=""1- Avg([areaspercut])""
                         DataType=""Auto""
                         DataMember=""areaview"" />
      </CalculatedFields>
    </SqlDataSource>
<SqlDataSource Name=""resourcesusageview"" ComponentName=""sqlDataSource4"">
  <Connection Name=""Metrics"" FromAppConfig=""true"" />
  <Query Type=""CustomSqlQuery"" Name=""resourcesusageview"">
    <Sql>
      SELECT
        ""Name"" AS ""ResourceName"",
        ""When"",
        ""workforcevsavailability"" AS ""resourceut"",
        ""worktime"",
        ""availabilitytime""
      FROM public.resourcesusageview_p('{PlanningId}'::uuid, '{WarehouseId}'::uuid, '{UtcOffset}', '{HourFormat}')
    </Sql>
  </Query>
  <ConnectionOptions CloseConnection=""true"" />
  <CalculatedFields>
    <CalculatedField Name=""Utilization""
                     Expression=""Iif([availabilitytime] = 0 &amp;&amp; [worktime]=0, Null, [resourceut])"" 
                     DataType=""Auto""
                     DataMember=""resourcesusageview"" />
                 
    <CalculatedField Name=""NonUtilization""
                     Expression=""Iif([availabilitytime] = 0 &amp;&amp; [worktime]=0, Null, Max(0, 1 - [resourceut]))""
                     DataType=""Auto""
                     DataMember=""resourcesusageview"" />
  </CalculatedFields>
</SqlDataSource>

  </DataSources>
  <Parameters>
    <Parameter Name=""HourInterval"" Type=""System.Int16"">
      <StaticListLookUpSettings>
        <Values>
          <Value>6</Value>
          <Value>8</Value>
        </Values>
      </StaticListLookUpSettings>
    </Parameter>
  </Parameters>
  <Items>
    <Grid ComponentName=""gridDashboardItem2"" Name=""Processes used status"" DataSource=""sqlDataSource1"" DataMember=""combinedplanningview"" FilterString=""Not([DataItem3] Is Null)"">
      <CustomProperties>
        <DashboardDescription KeyResource=""ProcessesUsedStatusWidgets"">Lists the start and end times of each process along with its type. This allows detailed tracking of process execution and supports performance analysis.</DashboardDescription>
      </CustomProperties>
      <DataItems>
        <Dimension DataMember=""InitDate_min"" DateTimeGroupInterval=""DateHourMinuteSecond"" DefaultId=""DataItem3"" />
        <Dimension DataMember=""EndDate_Max"" DateTimeGroupInterval=""DateHourMinuteSecond"" DefaultId=""DataItem1"" />
        <Dimension DataMember=""Today"" DateTimeGroupInterval=""DayMonthYear"" DefaultId=""DataItem2"" />
        <Dimension DataMember=""When"" DateTimeGroupInterval=""DayMonthYear"" DefaultId=""DataItem4"" />
        <Dimension DataMember=""InitDate_min"" DateTimeGroupInterval=""DateHourMinute"" DefaultId=""DataItem5"" />
        <Dimension DataMember=""ProcessName"" DefaultId=""DataItem0"" />
      </DataItems>
      <HiddenDimensions>
        <Dimension DefaultId=""DataItem2"" />
        <Dimension DefaultId=""DataItem4"" />
        <Dimension DefaultId=""DataItem5"" />
      </HiddenDimensions>
      <GridColumns>
        <GridDimensionColumn Name=""Start date"">
          <Dimension DefaultId=""DataItem3"" />
        </GridDimensionColumn>
        <GridDimensionColumn Name=""End date"">
          <Dimension DefaultId=""DataItem1"" />
        </GridDimensionColumn>
        <GridDimensionColumn Name=""Process"">
          <Dimension DefaultId=""DataItem0"" />
        </GridDimensionColumn>
      </GridColumns>
      <GridOptions />
      <ColumnFilterOptions ShowFilterRow=""true"" />
    </Grid>
    <Chart ComponentName=""chartDashboardItem2"" Name=""Process interleaving"" DataSource=""sqlDataSource2"" DataMember=""processinterleavingview"">
      <CustomProperties>
        <DashboardDescription KeyResource=""ProcessInterleavings"">Visualizes the distribution of time between different activities: inbound, outbound, interleaving, and inactive time. It helps understand how time is split across processes and highlights potential inefficiencies.</DashboardDescription>
      </CustomProperties>
      <DataItems>
        <Measure DataMember=""InboundSec"" DefaultId=""DataItem0"">
          <NumericFormat FormatType=""Percent"" />
        </Measure>
        <Measure DataMember=""OutboundSec"" DefaultId=""DataItem1"">
          <NumericFormat FormatType=""Percent"" />
        </Measure>
        <Measure DataMember=""InterleavingSec"" DefaultId=""DataItem2"">
          <NumericFormat FormatType=""Percent"" />
        </Measure>
        <Measure DataMember=""NoWorkingSec"" DefaultId=""DataItem3"">
          <NumericFormat FormatType=""Percent"" />
        </Measure>
        <Dimension DataMember=""When"" DateTimeGroupInterval=""DateHour"" DefaultId=""DataItem4"" />
        <Dimension DataMember=""When"" DateTimeGroupInterval=""DayMonthYear"" DefaultId=""DataItem5"" />
        <Dimension DataMember=""Today"" DateTimeGroupInterval=""DayMonthYear"" DefaultId=""DataItem6"" />
      </DataItems>
      <HiddenDimensions>
        <Dimension DefaultId=""DataItem5"" />
        <Dimension DefaultId=""DataItem6"" />
      </HiddenDimensions>
      <Arguments>
        <Argument DefaultId=""DataItem4"" />
      </Arguments>
      <Panes>
        <Pane Name=""Pane 1"">
          <AxisY TitleVisible=""false"" />
          <Series>
            <Simple Name=""Inbound time"" SeriesType=""FullStackedSplineArea"">
              <Value DefaultId=""DataItem0"" />
              <PointLabelOptions ContentType=""Value"" />
            </Simple>
            <Simple Name=""Outbound time"" SeriesType=""FullStackedSplineArea"">
              <Value DefaultId=""DataItem1"" />
              <PointLabelOptions ContentType=""Value"" />
            </Simple>
            <Simple Name=""Interleaving time"" SeriesType=""FullStackedSplineArea"">
              <Value DefaultId=""DataItem2"" />
              <PointLabelOptions ContentType=""Value"" />
            </Simple>
            <Simple Name=""Inactive time"" SeriesType=""FullStackedSplineArea"">
              <Value DefaultId=""DataItem3"" />
              <PointLabelOptions ContentType=""Value"" />
            </Simple>
          </Series>
        </Pane>
      </Panes>
    </Chart>
    <Chart ComponentName=""chartDashboardItem3"" Name=""Resources needed by processes "" DataSource=""sqlDataSource1"" DataMember=""combinedplanningview"" FilterString=""[DataItem2] = 'Operator'"" Rotated=""true"">
      <CustomProperties>
        <DashboardDescription KeyResource=""ResourcesByProcesses"">Shows the number of resources required for each process (e.g., inbound, shipping). It helps identify the demand of resources by process type, useful for planning and allocation.</DashboardDescription>
      </CustomProperties>
      <ColoringOptions MeasuresColoringMode=""Hue"" />
      <DataItems>
        <Measure DataMember=""ResourceId"" SummaryType=""CountDistinct"" DefaultId=""DataItem0"" />
        <Dimension DataMember=""ProcessName"" DefaultId=""DataItem1"" />
        <Dimension DataMember=""ResourceType"" DefaultId=""DataItem2"" />
      </DataItems>
      <HiddenDimensions>
        <Dimension DefaultId=""DataItem2"" />
      </HiddenDimensions>
      <Arguments>
        <Argument DefaultId=""DataItem1"" />
      </Arguments>
      <Panes>
        <Pane Name=""Pane 1"">
          <AxisY TitleVisible=""false"" />
          <Series>
            <Simple Name=""Resources"">
              <Value DefaultId=""DataItem0"" />
              <PointLabelOptions ContentType=""Value"" />
            </Simple>
          </Series>
        </Pane>
      </Panes>
      <AxisX EnableZooming=""true"" />
    </Chart>
    <Chart ComponentName=""chartDashboardItem4"" Name=""Resources usage"" DataSource=""sqlDataSource1"" DataMember=""combinedplanningview"">
        <CustomProperties>
            <DashboardDescription KeyResource=""ResourcesUsages"">Displays the actual number of resources currently in use. This provides a quick view of workforce or equipment engagement at a given moment.</DashboardDescription>
        </CustomProperties>
      <InteractivityOptions IsDrillDownEnabled=""true"" />
      <DataItems>
        <Measure DataMember=""ResourcesNotUsed"" DefaultId=""DataItem2"" />
        <Measure DataMember=""Resources_Count"" DefaultId=""DataItem3"" />
        <Dimension DataMember=""When"" DateTimeGroupInterval=""DateHour"" DefaultId=""DataItem1"" />
        <Dimension DataMember=""ResourceName"" DefaultId=""DataItem0"" />
      </DataItems>
      <Arguments>
        <Argument DefaultId=""DataItem0"" />
        <Argument DefaultId=""DataItem1"" />
      </Arguments>
      <Panes>
        <Pane Name=""Pane 1"">
          <AxisY TitleVisible=""false"" />
          <Series>
            <Simple Name=""Used resources"" SeriesType=""StackedBar"">
              <Value DefaultId=""DataItem3"" />
              <PointLabelOptions ContentType=""Value"" />
            </Simple>
            <Simple Name=""Unused resources"" SeriesType=""StackedBar"">
              <Value DefaultId=""DataItem2"" />
              <PointLabelOptions ContentType=""Value"" />
            </Simple>
          </Series>
        </Pane>
      </Panes>
      <AxisX EnableZooming=""true"" />
    </Chart>

     <Chart ComponentName=""chartDashboardItem1"" Name=""Resources utilization (%)""
       DataSource=""sqlDataSource4"" DataMember=""resourcesusageview"">
  <CustomProperties>
    <DashboardDescription KeyResource=""ResourcesUtilization"">
      Represents the percentage of available resources being used. It highlights efficiency and helps detect whether resources are over or underutilized.
    </DashboardDescription>
  </CustomProperties>

  <InteractivityOptions IsDrillDownEnabled=""true"" />
  <DataItems>
    <Dimension DataMember=""ResourceName"" DefaultId=""DataItem0"" />
    <Measure DataMember=""Utilization"" SummaryType=""Average"" DefaultId=""DataItem2"">
      <NumericFormat FormatType=""Percent"" />
    </Measure>
    <Measure DataMember=""NonUtilization"" SummaryType=""Average"" DefaultId=""DataItem3"">
      <NumericFormat FormatType=""Percent"" />
    </Measure>
    <Dimension DataMember=""When"" DateTimeGroupInterval=""DateHour"" DefaultId=""DataItem1"" />
  </DataItems>

  <Arguments>
    <Argument DefaultId=""DataItem0"" />
    <Argument DefaultId=""DataItem1"" />
  </Arguments>

  <Panes>
    <Pane Name=""Pane 1"">
      <AxisY TitleVisible=""false"" />
      <Series>
        <Simple Name=""Utilization time (%)"" SeriesType=""StackedBar"">
          <Value DefaultId=""DataItem2"" />
          <PointLabelOptions ContentType=""Value"" />
        </Simple>
        <Simple Name=""Non-Utilization time (%)"" SeriesType=""StackedBar"">
          <Value DefaultId=""DataItem3"" />
          <PointLabelOptions ContentType=""Value"" />
        </Simple>
      </Series>
    </Pane>
  </Panes>
  <AxisX EnableZooming=""true"" />
</Chart>
    <Chart ComponentName=""chartDashboardItem6"" Name=""Area utilization (%)"" DataSource=""sqlDataSource3"" DataMember=""areaview"">
      <CustomProperties>
        <DashboardDescription KeyResource=""AreaUtilization"">Shows the percentage of space utilization across different areas of the warehouse. It helps ensure that storage areas are efficiently used and prevents congestion.</DashboardDescription>
      </CustomProperties>
      <InteractivityOptions IsDrillDownEnabled=""true"" />
      <DataItems>
        <Dimension DataMember=""AreaName"" DefaultId=""DataItem0"" />
        <Measure DataMember=""areaspercut"" SummaryType=""Average"" DefaultId=""DataItem2"">
          <NumericFormat FormatType=""Percent"" />
        </Measure>
        <Measure DataMember=""NonUtilizationTime"" DefaultId=""DataItem3"">
          <NumericFormat FormatType=""Percent"" />
        </Measure>
        <Dimension DataMember=""When"" DateTimeGroupInterval=""DateHour"" DefaultId=""DataItem1"" />
      </DataItems>
      <Arguments>
        <Argument DefaultId=""DataItem0"" />
        <Argument DefaultId=""DataItem1"" />
      </Arguments>
      <Panes>
        <Pane Name=""Pane 1"">
          <AxisY TitleVisible=""false"" />
          <Series>
            <Simple Name=""Utilization time (%)"" SeriesType=""StackedBar"">
              <Value DefaultId=""DataItem2"" />
              <PointLabelOptions ContentType=""Value"" />
            </Simple>
            <Simple Name=""Non-Utilization time (%)"" SeriesType=""StackedBar"">
              <Value DefaultId=""DataItem3"" />
              <PointLabelOptions ContentType=""Value"" />
            </Simple>
          </Series>
        </Pane>
      </Panes>
      <AxisX EnableZooming=""true"" />
    </Chart>
  </Items>
  <ColorScheme>
    <Entry DataSource=""sqlDataSource1"" DataMember=""combinedplanningview"" Color=""-2595728"">
      <MeasureKey>
        <Definition DataMember=""UnusedResourcesC"" SummaryType=""Max"" />
      </MeasureKey>
    </Entry>
    <Entry DataSource=""sqlDataSource1"" DataMember=""combinedplanningview"" Color=""-11425852"">
      <DimensionKeys>
        <DimensionKey>
          <Definition DataMember=""ResourceName"" />
          <Value Type=""System.String"" Value=""8opvk79go4"" />
        </DimensionKey>
      </DimensionKeys>
    </Entry>
    <Entry DataSource=""sqlDataSource1"" DataMember=""combinedplanningview"" Color=""-2595728"">
      <DimensionKeys>
        <DimensionKey>
          <Definition DataMember=""ResourceName"" />
          <Value Type=""System.String"" Value=""dksulqqsr0"" />
        </DimensionKey>
      </DimensionKeys>
    </Entry>
    <Entry DataSource=""sqlDataSource1"" DataMember=""combinedplanningview"" Color=""-3944740"">
      <DimensionKeys>
        <DimensionKey>
          <Definition DataMember=""ResourceName"" />
          <Value Type=""System.String"" Value=""j1o0pb5euw"" />
        </DimensionKey>
      </DimensionKeys>
    </Entry>
    <Entry DataSource=""sqlDataSource1"" DataMember=""combinedplanningview"" Color=""-2595728"">
      <MeasureKey>
        <Definition DataMember=""ResourcesNotUsed"" SummaryType=""Count"" />
      </MeasureKey>
    </Entry>
    <Entry DataSource=""sqlDataSource1"" DataMember=""combinedplanningview"" Color=""-11425852"">
      <MeasureKey>
        <Definition DataMember=""ResourceId"" SummaryType=""CountDistinct"" />
      </MeasureKey>
    </Entry>
    <Entry DataSource=""sqlDataSource2"" DataMember=""processinterleavingview"" Color=""-8367720"">
      <MeasureKey>
        <Definition DataMember=""OutboundSec"" />
      </MeasureKey>
    </Entry>
    <Entry DataSource=""sqlDataSource2"" DataMember=""processinterleavingview"" Color=""-2380982"">
      <MeasureKey>
        <Definition DataMember=""InterleavingSec"" />
      </MeasureKey>
    </Entry>
    <Entry DataSource=""sqlDataSource2"" DataMember=""processinterleavingview"" Color=""-3944740"">
      <MeasureKey>
        <Definition DataMember=""NoWorkingSec"" />
      </MeasureKey>
    </Entry>
    <Entry DataSource=""sqlDataSource2"" DataMember=""processinterleavingview"" Color=""-11425852"">
      <MeasureKey>
        <Definition DataMember=""InboundSec"" />
      </MeasureKey>
    </Entry>
    <Entry DataSource=""sqlDataSource1"" DataMember=""combinedplanningview"" Color=""-11425852"">
      <MeasureKey>
        <Definition DataMember=""Resources_Count"" />
      </MeasureKey>
    </Entry>
    <Entry DataSource=""sqlDataSource1"" DataMember=""combinedplanningview"" Color=""-11425852"">
      <MeasureKey>
        <Definition DataMember=""SumSecPerWorker"" />
      </MeasureKey>
    </Entry>
    <Entry DataSource=""sqlDataSource1"" DataMember=""combinedplanningview"" Color=""-2595728"">
      <MeasureKey>
        <Definition DataMember=""NonUtilizationTime"" />
      </MeasureKey>
    </Entry>
    <Entry DataSource=""sqlDataSource3"" DataMember=""areaview"" Color=""-11425852"">
      <MeasureKey>
        <Definition DataMember=""areaspercut"" SummaryType=""Average"" />
      </MeasureKey>
    </Entry>
    <Entry DataSource=""sqlDataSource3"" DataMember=""areaview"" Color=""-2595728"">
      <MeasureKey>
        <Definition DataMember=""NonUtilizationTime"" />
      </MeasureKey>
    </Entry>
    <Entry DataSource=""sqlDataSource1"" DataMember=""combinedplanningview"" Color=""-11425852"">
      <MeasureKey>
        <Definition DataMember=""SumSecPerWorker"" SummaryType=""Average"" />
      </MeasureKey>
    </Entry>
    <Entry DataSource=""sqlDataSource1"" DataMember=""combinedplanningview"" Color=""-2595728"">
      <MeasureKey>
        <Definition DataMember=""ResourcesNotUsed"" />
      </MeasureKey>
    </Entry>
    <Entry DataSource=""sqlDataSource4"" DataMember=""resourcesusageview"" Color=""-2595728"">
      <MeasureKey>
        <Definition DataMember=""NonUtilization"" />
      </MeasureKey>
    </Entry>
    <Entry DataSource=""sqlDataSource4"" DataMember=""resourcesusageview"" Color=""-11425852"">
      <MeasureKey>
        <Definition DataMember=""resourcesut"" SummaryType=""Average"" />
      </MeasureKey>
    </Entry>
  </ColorScheme>
  <LayoutTree>
    <LayoutGroup>
      <LayoutGroup>
        <LayoutItem DashboardItem=""chartDashboardItem3"" Weight=""0.9126317395662673"" />
        <LayoutItem DashboardItem=""chartDashboardItem4"" Weight=""0.9126317395662673"" />
        <LayoutItem DashboardItem=""chartDashboardItem1"" Weight=""0.9126317395662673"" />
        <LayoutItem DashboardItem=""chartDashboardItem6"" Weight=""0.9126317395662673"" />
        <LayoutItem DashboardItem=""gridDashboardItem2""  Weight=""0.9126317395662673"" />
        <LayoutItem DashboardItem=""chartDashboardItem2"" Weight=""0.9126317395662673"" />
      </LayoutGroup>
    </LayoutGroup>
  </LayoutTree>
</Dashboard>";






        public static string view2 = @"<?xml version=""1.0"" encoding=""utf-8""?>
        <Dashboard>
        <Title Text=""VIEW2 - Recursos no necesarios por rango horario"" />
        <DataSources>
        <SqlDataSource Name=""combinedplanningview"" ComponentName=""sqlDataSource1"">
            <Connection Name=""Dashboard"" FromAppConfig=""true"" />
            <Query Type=""SelectQuery"" Name=""combinedplanningview"">
            <Tables>
                <Table Name=""planningresourcesview"" />
            </Tables>
            <Columns>
                <Column Table=""planningresourcesview"" Name=""When"" />
                <Column Table=""planningresourcesview"" Name=""InitDate"" />
                <Column Table=""planningresourcesview"" Name=""EndDate"" />
                <Column Table=""planningresourcesview"" Name=""ProcessId"" />
                <Column Table=""planningresourcesview"" Name=""ProcessName"" />
                <Column Table=""planningresourcesview"" Name=""ResourceType"" />
                <Column Table=""planningresourcesview"" Name=""ResourceId"" />
                <Column Table=""planningresourcesview"" Name=""ResourceName"" />
                <Column Table=""planningresourcesview"" Name=""AvailableResources"" />
            </Columns>
            </Query>
            <ConnectionOptions CloseConnection=""true"" />
            <CalculatedFields>
            <CalculatedField Name=""ResourcesNumber"" Expression=""CountNotNull([ResourceId])"" DataType=""Auto"" DataMember=""combinedplanningview"" />
            <CalculatedField Name=""AvailableResourcesC"" Expression=""Max(aggr(sum([AvailableResources]),[ResourceName], [When] ))- [ResourcesNumber] "" DataType=""Integer"" DataMember=""combinedplanningview"" />
            </CalculatedFields>
        </SqlDataSource>
        </DataSources>
        <Parameters>
        <Parameter Name=""HourInterval"" Type=""System.Int16"">
            <StaticListLookUpSettings>
            <Values>
                <Value>6</Value>
                <Value>8</Value>
            </Values>
            </StaticListLookUpSettings>
        </Parameter>
        </Parameters>
        <Items>
        <DateFilter ComponentName=""dateFilterDashboardItem2"" Name=""Date Filter 1"" ShowCaption=""false"" DataSource=""sqlDataSource1"" DataMember=""combinedplanningview"">
            <DataItems>
            <Dimension DataMember=""When"" DateTimeGroupInterval=""DayMonthYear"" DefaultId=""DataItem0"" />
            </DataItems>
            <Dimension DefaultId=""DataItem0"" />
            <DateTimePeriods>
            <DateTimePeriod Name=""Today"">
                <EndLimit>
                <FlowDateTimePeriodLimit Interval=""Day"" Offset=""1"" />
                </EndLimit>
            </DateTimePeriod>
            <DateTimePeriod Name=""Next two days"">
                <EndLimit>
                <FlowDateTimePeriodLimit Interval=""Day"" Offset=""2"" />
                </EndLimit>
            </DateTimePeriod>
            <DateTimePeriod Name=""Next 3 days"">
                <EndLimit>
                <FlowDateTimePeriodLimit Interval=""Day"" Offset=""3"" />
                </EndLimit>
            </DateTimePeriod>
            </DateTimePeriods>
        </DateFilter>
        <Grid ComponentName=""gridDashboardItem2"" Name=""Unused resources"" DataSource=""sqlDataSource1"" DataMember=""combinedplanningview"">
            <DataItems>
            <Dimension DataMember=""ResourceName"" DefaultId=""DataItem0"" />
            <Dimension DataMember=""ResourceType"" DefaultId=""DataItem1"" />
            <Dimension DataMember=""When"" DateTimeGroupInterval=""DateHour"" DefaultId=""DataItem2"" />
            <Measure DataMember=""AvailableResourcesC"" SummaryType=""Count"" DefaultId=""DataItem3"" />
            </DataItems>
            <GridColumns>
            <GridDimensionColumn Name=""Resource"">
                <Dimension DefaultId=""DataItem0"" />
            </GridDimensionColumn>
            <GridDimensionColumn Name=""Resource type"">
                <Dimension DefaultId=""DataItem1"" />
            </GridDimensionColumn>
            <GridSparklineColumn Name=""Resources"">
                <SparklineValue DefaultId=""DataItem3"" />
                <SparklineOptions ViewType=""WinLoss"" />
            </GridSparklineColumn>
            </GridColumns>
            <SparklineArgument DefaultId=""DataItem2"" />
            <GridOptions ColumnWidthMode=""AutoFitToContents"" />
            <ColumnFilterOptions />
        </Grid>
        <Chart ComponentName=""chartDashboardItem2"" Name=""Unused resources"" DataSource=""sqlDataSource1"" DataMember=""combinedplanningview"">
            <InteractivityOptions IsDrillDownEnabled=""true"" />
            <DataItems>
            <Dimension DataMember=""ResourceName"" Name=""Resource name"" DefaultId=""DataItem0"" />
            <Dimension DataMember=""When"" DateTimeGroupInterval=""DayMonthYear"" DefaultId=""DataItem2"" />
            <Dimension DataMember=""When"" DateTimeGroupInterval=""DateHour"" DefaultId=""DataItem3"" />
            <Dimension DataMember=""ResourcesNumber"" DefaultId=""DataItem1"" />
            <Dimension DataMember=""AvailableResources"" DefaultId=""DataItem4"" />
            <Measure DataMember=""AvailableResourcesC"" SummaryType=""Max"" DefaultId=""DataItem5"" />
            <Dimension DataMember=""ResourceType"" DefaultId=""DataItem6"" />
            </DataItems>
            <HiddenDimensions>
            <Dimension DefaultId=""DataItem1"" />
            <Dimension DefaultId=""DataItem4"" />
            </HiddenDimensions>
            <Arguments>
            <Argument DefaultId=""DataItem6"" />
            <Argument DefaultId=""DataItem0"" />
            <Argument DefaultId=""DataItem2"" />
            <Argument DefaultId=""DataItem3"" />
            </Arguments>
            <Panes>
            <Pane Name=""Pane 1"">
                <AxisY TitleVisible=""false"" />
                <Series>
                <Simple Name=""Unused resources"">
                    <Value DefaultId=""DataItem5"" />
                </Simple>
                </Series>
            </Pane>
            </Panes>
        </Chart>
        </Items>
        <LayoutTree>
        <LayoutGroup>
            <LayoutGroup Orientation=""Vertical"">
            <LayoutItem DashboardItem=""dateFilterDashboardItem2"" />
            <LayoutItem DashboardItem=""gridDashboardItem2"" Weight=""1.1933285138903236"" />
            <LayoutItem DashboardItem=""chartDashboardItem2"" Weight=""0.8066714861096764"" />
            </LayoutGroup>
        </LayoutGroup>
        </LayoutTree>
        </Dashboard>";

        public static string view3 = @"<?xml version=""1.0"" encoding=""utf-8""?>
        <Dashboard>
          <Title Text=""VIEW3 - Grado de no utilización de recursos"" />
          <DataSources>
            <SqlDataSource Name=""combinedplanningview"" ComponentName=""sqlDataSource1"">
              <Connection Name=""Dashboard"" FromAppConfig=""true"" />
              <Query Type=""SelectQuery"" Name=""combinedplanningview"">
                <Tables>
                  <Table Name=""planningresourcesview"" />
                </Tables>
                <Columns>
                  <Column Table=""planningresourcesview"" Name=""When"" />
                  <Column Table=""planningresourcesview"" Name=""InitDate"" />
                  <Column Table=""planningresourcesview"" Name=""EndDate"" />
                  <Column Table=""planningresourcesview"" Name=""ProcessId"" />
                  <Column Table=""planningresourcesview"" Name=""ProcessName"" />
                  <Column Table=""planningresourcesview"" Name=""ResourceType"" />
                  <Column Table=""planningresourcesview"" Name=""ResourceId"" />
                  <Column Table=""planningresourcesview"" Name=""ResourceName"" />
                  <Column Table=""planningresourcesview"" Name=""AvailableResources"" />
                </Columns>
              </Query>
              <ConnectionOptions CloseConnection=""true"" />
              <CalculatedFields>
                <CalculatedField Name=""ResourcesNumber"" Expression=""CountNotNull([ResourceId])"" DataType=""Auto"" DataMember=""combinedplanningview"" />
                <CalculatedField Name=""AvailableResourcesC"" Expression=""Max(aggr(sum([AvailableResources]),[ResourceName], [When] ))- [ResourcesNumber] "" DataType=""Integer"" DataMember=""combinedplanningview"" />
              </CalculatedFields>
            </SqlDataSource>
          </DataSources>
          <Parameters>
            <Parameter Name=""HourInterval"" Type=""System.Int16"">
              <StaticListLookUpSettings>
                <Values>
                  <Value>6</Value>
                  <Value>8</Value>
                </Values>
              </StaticListLookUpSettings>
            </Parameter>
          </Parameters>
          <Items>
            <DateFilter ComponentName=""dateFilterDashboardItem3"" Name=""Date Filter 1"" ShowCaption=""false"" DataSource=""sqlDataSource1"" DataMember=""combinedplanningview"">
              <DataItems>
                <Dimension DataMember=""When"" DateTimeGroupInterval=""DayMonthYear"" DefaultId=""DataItem0"" />
              </DataItems>
              <Dimension DefaultId=""DataItem0"" />
              <DateTimePeriods>
                <DateTimePeriod Name=""Today"">
                  <EndLimit>
                    <FlowDateTimePeriodLimit Interval=""Day"" Offset=""1"" />
                  </EndLimit>
                </DateTimePeriod>
                <DateTimePeriod Name=""Next two days"">
                  <EndLimit>
                    <FlowDateTimePeriodLimit Interval=""Day"" Offset=""2"" />
                  </EndLimit>
                </DateTimePeriod>
                <DateTimePeriod Name=""Next 3 days"">
                  <EndLimit>
                    <FlowDateTimePeriodLimit Interval=""Day"" Offset=""3"" />
                  </EndLimit>
                </DateTimePeriod>
              </DateTimePeriods>
            </DateFilter>
            <Grid ComponentName=""gridDashboardItem3"" Name=""Unused resources"" DataSource=""sqlDataSource1"" DataMember=""combinedplanningview"">
              <DataItems>
                <Dimension DataMember=""ResourceName"" DefaultId=""DataItem0"" />
                <Dimension DataMember=""ResourceType"" DefaultId=""DataItem1"" />
                <Dimension DataMember=""When"" DateTimeGroupInterval=""DateHour"" DefaultId=""DataItem2"" />
                <Measure DataMember=""AvailableResourcesC"" DefaultId=""DataItem3"" />
              </DataItems>
              <GridColumns>
                <GridDimensionColumn>
                  <Dimension DefaultId=""DataItem2"" />
                </GridDimensionColumn>
                <GridDimensionColumn Name=""Resource"">
                  <Dimension DefaultId=""DataItem0"" />
                </GridDimensionColumn>
                <GridDimensionColumn Name=""Resource type"">
                  <Dimension DefaultId=""DataItem1"" />
                </GridDimensionColumn>
                <GridMeasureColumn Name=""Unused resources"">
                  <Measure DefaultId=""DataItem3"" />
                </GridMeasureColumn>
              </GridColumns>
              <GridOptions ColumnWidthMode=""AutoFitToContents"" />
              <ColumnFilterOptions />
            </Grid>
            <Pivot ComponentName=""pivotDashboardItem3"" Name=""Unused resources"" DataSource=""sqlDataSource1"" DataMember=""combinedplanningview"">
              <DataItems>
                <Dimension DataMember=""When"" SortOrder=""Descending"" DateTimeGroupInterval=""DateHour"" SortByMeasure=""DataItem3"" DefaultId=""DataItem0"" />
                <Dimension DataMember=""ResourceName"" Name=""Resources"" DefaultId=""DataItem1"" />
                <Dimension DataMember=""ResourceType"" Name=""Resource type"" DefaultId=""DataItem2"" />
                <Measure DataMember=""AvailableResourcesC"" Name=""Unused resources"" DefaultId=""DataItem3"" />
              </DataItems>
              <Rows>
                <Row DefaultId=""DataItem0"" />
                <Row DefaultId=""DataItem2"" />
                <Row DefaultId=""DataItem1"" />
              </Rows>
              <Values>
                <Value DefaultId=""DataItem3"" />
              </Values>
            </Pivot>
            <Chart ComponentName=""chartDashboardItem3"" Name=""Unused resources"" DataSource=""sqlDataSource1"" DataMember=""combinedplanningview"" Rotated=""true"">
              <InteractivityOptions IsDrillDownEnabled=""true"" />
              <DataItems>
                <Dimension DataMember=""When"" SortOrder=""Descending"" DateTimeGroupInterval=""DateHour"" SortByMeasure=""DataItem1"" DefaultId=""DataItem0"" />
                <Measure DataMember=""AvailableResourcesC"" DefaultId=""DataItem1"" />
                <Dimension DataMember=""ResourceType"" Name=""Resource type"" SortOrder=""Descending"" SortByMeasure=""DataItem1"" DefaultId=""DataItem2"" />
                <Dimension DataMember=""ResourceName"" Name=""Resources"" SortOrder=""Descending"" SortByMeasure=""DataItem1"" DefaultId=""DataItem3"" />
              </DataItems>
              <Arguments>
                <Argument DefaultId=""DataItem0"" />
                <Argument DefaultId=""DataItem2"" />
                <Argument DefaultId=""DataItem3"" />
              </Arguments>
              <Panes>
                <Pane Name=""Pane 1"">
                  <AxisY TitleVisible=""false"" />
                  <Series>
                    <Simple Name=""Unused resources"">
                      <Value DefaultId=""DataItem1"" />
                    </Simple>
                  </Series>
                </Pane>
              </Panes>
            </Chart>
          </Items>
          <LayoutTree>
            <LayoutGroup>
              <LayoutGroup Orientation=""Vertical"">
                <LayoutItem DashboardItem=""dateFilterDashboardItem3"" />
                <LayoutGroup Orientation=""Vertical"" Weight=""1.1933285138903236"">
                  <LayoutGroup Weight=""0.7865029457323972"">
                    <LayoutItem DashboardItem=""gridDashboardItem3"" Weight=""1.1933285138903236"" />
                    <LayoutItem DashboardItem=""pivotDashboardItem3"" Weight=""1.1933285138903236"" />
                  </LayoutGroup>
                  <LayoutItem DashboardItem=""chartDashboardItem3"" Weight=""1.6001540820482498"" />
                </LayoutGroup>
              </LayoutGroup>
            </LayoutGroup>
          </LayoutTree>
        </Dashboard>";

        public static string view4 = @"<?xml version=""1.0"" encoding=""utf-8""?>
        <Dashboard>
        <Title Text=""VIEW4 - Grado de utilización de recursos"" />
        <DataSources>
        <SqlDataSource Name=""combinedplanningview"" ComponentName=""sqlDataSource1"">
            <Connection Name=""Dashboard"" FromAppConfig=""true"" />
            <Query Type=""SelectQuery"" Name=""combinedplanningview"">
            <Tables>
                <Table Name=""planningresourcesview"" />
            </Tables>
            <Columns>
                <Column Table=""planningresourcesview"" Name=""When"" />
                <Column Table=""planningresourcesview"" Name=""InitDate"" />
                <Column Table=""planningresourcesview"" Name=""EndDate"" />
                <Column Table=""planningresourcesview"" Name=""ProcessId"" />
                <Column Table=""planningresourcesview"" Name=""ProcessName"" />
                <Column Table=""planningresourcesview"" Name=""ResourceType"" />
                <Column Table=""planningresourcesview"" Name=""ResourceId"" />
                <Column Table=""planningresourcesview"" Name=""ResourceName"" />
                <Column Table=""planningresourcesview"" Name=""AvailableResources"" />
            </Columns>
            </Query>
            <ConnectionOptions CloseConnection=""true"" />
            <CalculatedFields>
            <CalculatedField Name=""ResourcesNumber"" Expression=""CountNotNull([ResourceId])"" DataType=""Auto"" DataMember=""combinedplanningview"" />
            <CalculatedField Name=""AvailableResourcesC"" Expression=""Max(aggr(sum([AvailableResources]),[ResourceName], [When] ))- [ResourcesNumber] "" DataType=""Integer"" DataMember=""combinedplanningview"" />
            </CalculatedFields>
        </SqlDataSource>
        </DataSources>
        <Parameters>
        <Parameter Name=""HourInterval"" Type=""System.Int16"">
            <StaticListLookUpSettings>
            <Values>
                <Value>6</Value>
                <Value>8</Value>
            </Values>
            </StaticListLookUpSettings>
        </Parameter>
        </Parameters>
        <Items>
        <DateFilter ComponentName=""dateFilterDashboardItem4"" Name=""Date Filter 1"" ShowCaption=""false"" DataSource=""sqlDataSource1"" DataMember=""combinedplanningview"">
            <DataItems>
            <Dimension DataMember=""When"" DateTimeGroupInterval=""DayMonthYear"" DefaultId=""DataItem0"" />
            </DataItems>
            <Dimension DefaultId=""DataItem0"" />
            <DateTimePeriods>
            <DateTimePeriod Name=""Today"">
                <EndLimit>
                <FlowDateTimePeriodLimit Interval=""Day"" Offset=""1"" />
                </EndLimit>
            </DateTimePeriod>
            <DateTimePeriod Name=""Next two days"">
                <EndLimit>
                <FlowDateTimePeriodLimit Interval=""Day"" Offset=""2"" />
                </EndLimit>
            </DateTimePeriod>
            <DateTimePeriod Name=""Next 3 days"">
                <EndLimit>
                <FlowDateTimePeriodLimit Interval=""Day"" Offset=""3"" />
                </EndLimit>
            </DateTimePeriod>
            </DateTimePeriods>
        </DateFilter>
        <Grid ComponentName=""gridDashboardItem4"" Name=""Used resources"" DataSource=""sqlDataSource1"" DataMember=""combinedplanningview"">
            <DataItems>
            <Dimension DataMember=""ResourceName"" DefaultId=""DataItem0"" />
            <Dimension DataMember=""ResourceType"" DefaultId=""DataItem1"" />
            <Dimension DataMember=""When"" DateTimeGroupInterval=""DateHour"" DefaultId=""DataItem2"" />
            <Measure DataMember=""ResourcesNumber"" SummaryType=""Count"" DefaultId=""DataItem3"" />
            </DataItems>
            <GridColumns>
            <GridDimensionColumn>
                <Dimension DefaultId=""DataItem2"" />
            </GridDimensionColumn>
            <GridDimensionColumn Name=""Resource type"">
                <Dimension DefaultId=""DataItem1"" />
            </GridDimensionColumn>
            <GridDimensionColumn Name=""Resource"">
                <Dimension DefaultId=""DataItem0"" />
            </GridDimensionColumn>
            <GridMeasureColumn Name=""Used resources"">
                <Measure DefaultId=""DataItem3"" />
            </GridMeasureColumn>
            </GridColumns>
            <GridOptions ColumnWidthMode=""AutoFitToContents"" />
            <ColumnFilterOptions />
        </Grid>
        <Pivot ComponentName=""pivotDashboardItem4"" Name=""Unused resources"" DataSource=""sqlDataSource1"" DataMember=""combinedplanningview"">
            <DataItems>
            <Dimension DataMember=""When"" SortOrder=""Descending"" DateTimeGroupInterval=""DateHour"" SortByMeasure=""DataItem3"" DefaultId=""DataItem0"" />
            <Dimension DataMember=""ResourceName"" Name=""Resources"" DefaultId=""DataItem1"" />
            <Dimension DataMember=""ResourceType"" Name=""Resource type"" DefaultId=""DataItem2"" />
            <Measure DataMember=""ResourcesNumber"" Name=""Used resources"" DefaultId=""DataItem3"" />
            </DataItems>
            <Rows>
            <Row DefaultId=""DataItem0"" />
            <Row DefaultId=""DataItem2"" />
            <Row DefaultId=""DataItem1"" />
            </Rows>
            <Values>
            <Value DefaultId=""DataItem3"" />
            </Values>
        </Pivot>
        <Chart ComponentName=""chartDashboardItem4"" Name=""Unused resources"" DataSource=""sqlDataSource1"" DataMember=""combinedplanningview"" Rotated=""true"">
            <InteractivityOptions IsDrillDownEnabled=""true"" />
            <DataItems>
            <Dimension DataMember=""When"" SortOrder=""Descending"" DateTimeGroupInterval=""DateHour"" SortByMeasure=""DataItem1"" DefaultId=""DataItem0"" />
            <Measure DataMember=""ResourcesNumber"" DefaultId=""DataItem1"" />
            <Dimension DataMember=""ResourceType"" Name=""Resource type"" SortOrder=""Descending"" SortByMeasure=""DataItem1"" DefaultId=""DataItem2"" />
            <Dimension DataMember=""ResourceName"" Name=""Resources"" SortOrder=""Descending"" SortByMeasure=""DataItem1"" DefaultId=""DataItem3"" />
            </DataItems>
            <Arguments>
            <Argument DefaultId=""DataItem0"" />
            <Argument DefaultId=""DataItem2"" />
            <Argument DefaultId=""DataItem3"" />
            </Arguments>
            <Panes>
            <Pane Name=""Pane 1"">
                <AxisY TitleVisible=""false"" />
                <Series>
                <Simple Name=""Used resources"">
                    <Value DefaultId=""DataItem1"" />
                </Simple>
                </Series>
            </Pane>
            </Panes>
        </Chart>
        </Items>
        <LayoutTree>
        <LayoutGroup>
            <LayoutGroup Orientation=""Vertical"">
            <LayoutItem DashboardItem=""dateFilterDashboardItem4"" />
            <LayoutGroup Orientation=""Vertical"" Weight=""1.1933285138903236"">
                <LayoutGroup Weight=""0.7865029457323972"">
                <LayoutItem DashboardItem=""gridDashboardItem4"" Weight=""1.1933285138903236"" />
                <LayoutItem DashboardItem=""pivotDashboardItem4"" Weight=""1.1933285138903236"" />
                </LayoutGroup>
                <LayoutItem DashboardItem=""chartDashboardItem4"" Weight=""1.6001540820482498"" />
            </LayoutGroup>
            </LayoutGroup>
        </LayoutGroup>
        </LayoutTree>
        </Dashboard>";

        public static string view5 = @"<?xml version=""1.0"" encoding=""utf-8""?>
        <Dashboard>
          <Title Text=""VIEW 5 - Estado de procesos utilizados"" />
          <DataSources>
            <SqlDataSource Name=""planningresourcesview"" ComponentName=""sqlDataSource1"">
              <Connection Name=""Dashboard"" FromAppConfig=""true"" />
              <Query Type=""SelectQuery"" Name=""planningresourcesview"">
                <Tables>
                  <Table Name=""planningresourcesview"" />
                </Tables>
                <Columns>
                  <Column Table=""planningresourcesview"" Name=""InitDate"" />
                  <Column Table=""planningresourcesview"" Name=""EndDate"" />
                  <Column Table=""planningresourcesview"" Name=""ProcessId"" />
                  <Column Table=""planningresourcesview"" Name=""ProcessName"" />
                </Columns>
              </Query>
              <ConnectionOptions CloseConnection=""true"" />
            </SqlDataSource>
          </DataSources>
          <Items>
            <Grid ComponentName=""gridDashboardItem5"" Name=""Processes used status"" DataSource=""sqlDataSource1"" DataMember=""planningresourcesview"">
              <DataItems>
                <Dimension DataMember=""ProcessName"" DefaultId=""DataItem0"" />
                <Dimension DataMember=""InitDate"" DateTimeGroupInterval=""DateHourMinute"" DefaultId=""DataItem1"" />
                <Dimension DataMember=""EndDate"" DateTimeGroupInterval=""DateHourMinute"" DefaultId=""DataItem2"" />
              </DataItems>
              <GridColumns>
                <GridDimensionColumn Name=""Process name"">
                  <Dimension DefaultId=""DataItem0"" />
                </GridDimensionColumn>
                <GridDimensionColumn Name=""Start date"">
                  <Dimension DefaultId=""DataItem1"" />
                </GridDimensionColumn>
                <GridDimensionColumn Name=""End date"">
                  <Dimension DefaultId=""DataItem2"" />
                </GridDimensionColumn>
              </GridColumns>
              <GridOptions />
              <ColumnFilterOptions />
            </Grid>
          </Items>
          <LayoutTree>
            <LayoutGroup>
              <LayoutItem DashboardItem=""gridDashboardItem5"" />
            </LayoutGroup>
          </LayoutTree>
        </Dashboard>";

        public static string view6 = @"<?xml version=""1.0"" encoding=""utf-8""?>
        <Dashboard>
          <Title Text=""VIEW6 - Grado de intercalado de tareas"" />
          <DataSources>
            <SqlDataSource Name=""processinterleavingview"" ComponentName=""sqlDataSource1"">
              <Connection Name=""Dashboard"" FromAppConfig=""true"" />
              <Query Type=""SelectQuery"" Name=""processinterleavingview"">
                <Tables>
                  <Table Name=""processinterleavingview"" />
                </Tables>
                <Columns>
                  <Column Table=""processinterleavingview"" Name=""When"" />
                  <Column Table=""processinterleavingview"" Name=""InboundSec"" />
                  <Column Table=""processinterleavingview"" Name=""OutboundSec"" />
                  <Column Table=""processinterleavingview"" Name=""InterleavingSec"" />
                </Columns>
              </Query>
              <ConnectionOptions CloseConnection=""true"" />
            </SqlDataSource>
          </DataSources>
          <Items>
            <Grid ComponentName=""gridDashboardItem6"" Name=""Processes interleaving time "" DataSource=""sqlDataSource1"" DataMember=""processinterleavingview"">
              <DataItems>
                <Dimension DataMember=""When"" DateTimeGroupInterval=""DateHour"" DefaultId=""DataItem1"" />
                <Dimension DataMember=""InterleavingSec"" DefaultId=""DataItem3"">
                  <NumericFormat FormatType=""Number"" Unit=""Ones"" />
                </Dimension>
                <Dimension DataMember=""OutboundSec"" DefaultId=""DataItem4"">
                  <NumericFormat FormatType=""Number"" Unit=""Ones"" />
                </Dimension>
                <Dimension DataMember=""InboundSec"" DefaultId=""DataItem2"">
                  <NumericFormat FormatType=""Number"" Unit=""Ones"" />
                </Dimension>
              </DataItems>
              <GridColumns>
                <GridDimensionColumn>
                  <Dimension DefaultId=""DataItem1"" />
                </GridDimensionColumn>
                <GridDimensionColumn Name=""Inbound time(seconds)"">
                  <Dimension DefaultId=""DataItem2"" />
                </GridDimensionColumn>
                <GridDimensionColumn Name=""Outbound time (seconds)"">
                  <Dimension DefaultId=""DataItem4"" />
                </GridDimensionColumn>
                <GridDimensionColumn Name=""Interleaving time (seconds)"">
                  <Dimension DefaultId=""DataItem3"" />
                </GridDimensionColumn>
              </GridColumns>
              <GridOptions />
              <ColumnFilterOptions />
            </Grid>
            <DateFilter ComponentName=""dateFilterDashboardItem6"" Name=""Date Filter 1"" ShowCaption=""false"" DataSource=""sqlDataSource1"" DataMember=""processinterleavingview"">
              <DataItems>
                <Dimension DataMember=""When"" DateTimeGroupInterval=""DayMonthYear"" DefaultId=""DataItem0"" />
              </DataItems>
              <Dimension DefaultId=""DataItem0"" />
              <DateTimePeriods>
                <DateTimePeriod Name=""Today"">
                  <EndLimit>
                    <FlowDateTimePeriodLimit Interval=""Day"" Offset=""1"" />
                  </EndLimit>
                </DateTimePeriod>
                <DateTimePeriod Name=""Next two days"">
                  <EndLimit>
                    <FlowDateTimePeriodLimit Interval=""Day"" Offset=""2"" />
                  </EndLimit>
                </DateTimePeriod>
                <DateTimePeriod Name=""Next three days"">
                  <EndLimit>
                    <FlowDateTimePeriodLimit Interval=""Day"" Offset=""3"" />
                  </EndLimit>
                </DateTimePeriod>
              </DateTimePeriods>
            </DateFilter>
          </Items>
          <LayoutTree>
            <LayoutGroup>
              <LayoutGroup Orientation=""Vertical"">
                <LayoutItem DashboardItem=""dateFilterDashboardItem6"" Weight=""0.1572859650934628"" />
                <LayoutItem DashboardItem=""gridDashboardItem6"" Weight=""1.8427140349065372"" />
              </LayoutGroup>
            </LayoutGroup>
          </LayoutTree>
        </Dashboard>";

        public static string WFMLaborWorkerPerProcessType = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Dashboard>
  <Title Text=""VIEW7"" />
  <DataSources>
    <SqlDataSource Name=""SQL Data Source"" ComponentName=""sqlDataSource1"">
      <Connection Name=""Metrics"" FromAppConfig=""true"" />
      
      <Query Type=""CustomSqlQuery"" Name=""WFMLaborWorkerPerProcessType"">
        <Sql>
          SELECT
            wpp.""Id"",
            wpp.""ScheduleId"",
            wpp.""ProcessType"",
            wpp.""PlanningId"",
            wpp.""WFMLaborPerFlowId"",
            wpp.""WorkTime"",
            wpp.""Efficiency"",
            wpp.""Productivity"",
            wpp.""Utility"",
            wpp.""TotalProductivity"",
            wpp.""TotalUtility"",
            wpp.""Breaks"",
            wpp.""Ranking"",
            wpp.""TotalOrders"",
            wpp.""ClosedOrders"",
            wpp.""TotalProcesses"",
            wpp.""ClosedProcesses"",
            w.""Name"",
            wpp.""WorkerId"",
            wpp.""Progress"",
            p.""CreationDate"",
            s.""Name"" AS ""Schedules_Name""
          FROM public.""WFMLaborWorkerPerProcessType"" wpp
          JOIN public.""Workers""   w ON wpp.""WorkerId""   = w.""Id""
          JOIN public.""Plannings"" p ON wpp.""PlanningId"" = p.""Id""
          JOIN public.""Schedules"" s ON wpp.""ScheduleId"" = s.""Id""
          WHERE wpp.""PlanningId"" = '{PlanningId}'::uuid
        </Sql>
      </Query>
      <Query Type=""CustomSqlQuery"" Name=""workforcevsavailabilityview"">
        <Sql>
          SELECT
            ""When"",
            ""workforcevsavailability"",
            ""Name""
          FROM public.workforcevsavailabilityview_p('{PlanningId}'::uuid, '{WarehouseId}'::uuid, '{UtcOffset}', '{HourFormat}')
        </Sql>
      </Query>
      <Query Type=""CustomSqlQuery"" Name=""WFMLaborWorker"">
        <Sql>
          SELECT
            wfw.""Id"",
            wfw.""WorkerId"",
            wfw.""ScheduleId"",
            wfw.""PlanningId"",
            wfw.""WorkTime"",
            wfw.""Efficiency"",
            wfw.""Productivity"",
            wfw.""Utility"",
            wfw.""TotalProductivity"",
            wfw.""TotalUtility"",
            wfw.""Breaks"",
            wfw.""Ranking"",
            wfw.""TotalOrders"",
            wfw.""ClosedOrders"",
            wfw.""TotalProcesses"",
            wfw.""ClosedProcesses"",
            wfw.""Progress"",
            w.""Name"",
            t.""Name"" AS ""Teams_Name"",
            p.""CreationDate""
          FROM public.""WFMLaborWorker"" wfw
          JOIN public.""Workers""   w ON wfw.""WorkerId""   = w.""Id""
          JOIN public.""Teams""     t ON w.""TeamId""       = t.""Id""
          JOIN public.""Plannings"" p ON wfw.""PlanningId"" = p.""Id""
          WHERE wfw.""PlanningId"" = '{PlanningId}'::uuid
        </Sql>
      </Query>

      <ConnectionOptions CloseConnection=""true"" />
      <CalculatedFields>
        <CalculatedField Name=""MaxPlanning"" Expression=""aggr(Max([CreationDate] ))"" DataType=""Auto"" DataMember=""WFMLaborWorkerPerProcessType"" />
        <CalculatedField Name=""WorkTimePerMinutes"" Expression=""[WorkTime]/60"" DataType=""Auto"" DataMember=""WFMLaborWorkerPerProcessType"" />
        <CalculatedField Name=""MaxPlanningWFMLaborWorker"" Expression=""aggr(Max( [CreationDate]  ))"" DataType=""Auto"" DataMember=""WFMLaborWorker"" />
      </CalculatedFields>
    </SqlDataSource>
  </DataSources>

  <Parameters>
    <Parameter Name=""PlanningId"" Type=""System.String"" />
  </Parameters>

  <Items>
    <Chart ComponentName=""chartDashboardItem1"" Name=""Work time (min) by worker and process"" DataSource=""sqlDataSource1"" DataMember=""WFMLaborWorkerPerProcessType"" FilterString=""[DataItem6] = [DataItem5]"" Rotated=""true"">
      <CustomProperties>
        <DashboardDescription KeyResource=""WFMLaborWorkerPerProcessDesc"">Displays the amount of time (in minutes) each worker spends on different processes. This allows you to compare workload distribution and identify where time is being invested across tasks.</DashboardDescription>
      </CustomProperties>
      <DataItems>
        <Dimension DataMember=""WorkerId"" DefaultId=""DataItem0"" />
        <Dimension DataMember=""ProcessType"" DefaultId=""DataItem1"" />
        <Measure DataMember=""WorkTimePerMinutes"" DefaultId=""DataItem2"" />
        <Dimension DataMember=""Name"" SortByMeasure=""DataItem2"" DefaultId=""DataItem3"" />
        <Dimension DataMember=""ProcessType"" ColoringMode=""Hue"" DefaultId=""DataItem4"" />
        <Dimension DataMember=""MaxPlanning"" DateTimeGroupInterval=""None"" DefaultId=""DataItem5"" />
        <Dimension DataMember=""CreationDate"" DateTimeGroupInterval=""None"" DefaultId=""DataItem6"" />
      </DataItems>
      <HiddenDimensions>
        <Dimension DefaultId=""DataItem5"" />
        <Dimension DefaultId=""DataItem6"" />
      </HiddenDimensions>
      <SeriesDimensions>
        <SeriesDimension DefaultId=""DataItem4"" />
      </SeriesDimensions>
      <Arguments>
        <Argument DefaultId=""DataItem3"" />
      </Arguments>
      <Panes>
        <Pane Name=""Pane 1"">
          <AxisY TitleVisible=""false"" />
          <Series>
            <Simple SeriesType=""StackedBar"">
              <Value DefaultId=""DataItem2"" />
              <PointLabelOptions ContentType=""Value"" />
            </Simple>
          </Series>
        </Pane>
      </Panes>
      <AxisX EnableZooming=""true"" VisiblePointsCount=""4"" />
    </Chart>

    <Pivot ComponentName=""pivotDashboardItem1"" Name=""KPI (%) by team and worker"" DataSource=""sqlDataSource1"" DataMember=""WFMLaborWorker"" FilterString=""[DataItem6] = [DataItem5]"">
      <CustomProperties>
        <DashboardDescription KeyResource=""WFMLaborWorkerDesc"">Presents key performance indicators -Efficiency, Productivity, and Utility- for each team and worker. It provides a quick view of how individuals and teams are performing against expectations.</DashboardDescription>
      </CustomProperties>
      <DataItems>
        <Dimension DataMember=""Name"" SortByMeasure=""DataItem3"" DefaultId=""DataItem0"" />
        <Measure DataMember=""Productivity"" Name=""Productivity"" SummaryType=""Average"" DefaultId=""DataItem1"" />
        <Measure DataMember=""Utility"" Name=""Utility"" SummaryType=""Average"" DefaultId=""DataItem2"" />
        <Measure DataMember=""Efficiency"" Name=""Efficiency"" SummaryType=""Average"" DefaultId=""DataItem3"" />
        <Dimension DataMember=""Teams_Name"" SortOrder=""Descending"" SortByMeasure=""DataItem3"" DefaultId=""DataItem4"" />
        <Dimension DataMember=""MaxPlanningWFMLaborWorker"" DateTimeGroupInterval=""None"" DefaultId=""DataItem5"" />
        <Dimension DataMember=""CreationDate"" DateTimeGroupInterval=""None"" DefaultId=""DataItem6"" />
      </DataItems>
      <HiddenDimensions>
        <Dimension DefaultId=""DataItem5"" />
        <Dimension DefaultId=""DataItem6"" />
      </HiddenDimensions>
      <FormatRules>
        <PivotItemFormatRule Name=""Format Rule 1"" DataItem=""DataItem3"" DataItemApplyTo=""DataItem3"" IntersectionLevelMode=""AllLevels"">
          <FormatConditionRangeSet ValueType=""Number"">
            <RangeSet>
              <Ranges>
                <RangeInfo>
                  <Value Type=""System.Decimal"" Value=""0"" />
                  <IconSettings IconType=""DirectionalRedTriangleDown"" />
                </RangeInfo>
                <RangeInfo>
                  <Value Type=""System.Decimal"" Value=""90"" />
                  <IconSettings IconType=""DirectionalYellowDash"" />
                </RangeInfo>
                <RangeInfo>
                  <Value Type=""System.Decimal"" Value=""98"" />
                  <IconSettings IconType=""DirectionalGreenTriangleUp"" />
                </RangeInfo>
              </Ranges>
            </RangeSet>
          </FormatConditionRangeSet>
          <PivotItemFormatRuleLevel />
        </PivotItemFormatRule>
      </FormatRules>
      <Rows>
        <Row DefaultId=""DataItem4"" />
        <Row DefaultId=""DataItem0"" />
      </Rows>
      <Values>
        <Value DefaultId=""DataItem3"" />
        <Value DefaultId=""DataItem1"" />
        <Value DefaultId=""DataItem2"" />
      </Values>
    </Pivot>

    <Chart ComponentName=""chartDashboardItem3"" Name=""Top 10 workers with less work time (min)"" DataSource=""sqlDataSource1"" DataMember=""WFMLaborWorkerPerProcessType"" Rotated=""true"">
      <CustomProperties>
        <DashboardDescription KeyResource=""WFMLaborWorkerPerProcessDesc"">Lists the ten workers with the lowest total work time in minutes. This can highlight potential underutilization, workload imbalances, or opportunities to reassign tasks.</DashboardDescription>
      </CustomProperties>
      <ColoringOptions MeasuresColoringMode=""Hue"" />
      <DataItems>
        <Dimension DataMember=""MaxPlanning"" DateTimeGroupInterval=""None"" DefaultId=""DataItem0"" />
        <Dimension DataMember=""CreationDate"" DateTimeGroupInterval=""None"" DefaultId=""DataItem1"" />
        <Dimension DataMember=""Name"" TopNEnabled=""true"" TopNCount=""10"" TopNMode=""Bottom"" TopNMeasure=""DataItem3"" DefaultId=""DataItem2"" />
        <Measure DataMember=""WorkTimePerMinutes"" FilterString=""[DataItem1] = [DataItem0]"" DefaultId=""DataItem3"" />
      </DataItems>
      <HiddenDimensions>
        <Dimension DefaultId=""DataItem0"" />
        <Dimension DefaultId=""DataItem1"" />
      </HiddenDimensions>
      <Arguments>
        <Argument DefaultId=""DataItem2"" />
      </Arguments>
      <Panes>
        <Pane Name=""Pane 1"">
          <AxisY TitleVisible=""false"" />
          <Series>
            <Simple Name=""Work time"">
              <Value DefaultId=""DataItem3"" />
              <PointLabelOptions ContentType=""Value"" />
            </Simple>
          </Series>
        </Pane>
      </Panes>
      <AxisX EnableZooming=""true"" VisiblePointsCount=""4"" />
    </Chart>

    <Chart ComponentName=""chartDashboardItem2"" Name=""Workforce vs availability (%) by role and hour"" DataSource=""sqlDataSource1"" DataMember=""workforcevsavailabilityview"">
      <CustomProperties>
        <DashboardDescription KeyResource=""WorkforceVSAvailabilitydesc"">Shows the relationship between workforce utilization and availability, broken down by role and hour. It helps visualize when resources are over- or underutilized throughout the day.</DashboardDescription>
      </CustomProperties>
      <DataItems>
        <Dimension DataMember=""When"" DateTimeGroupInterval=""DateHour"" DefaultId=""DataItem1"" />
        <Measure DataMember=""workforcevsavailability"" DefaultId=""DataItem2"">
          <NumericFormat FormatType=""Percent"" />
        </Measure>
        <Dimension DataMember=""Name"" DefaultId=""DataItem0"" />
      </DataItems>
      <SeriesDimensions>
        <SeriesDimension DefaultId=""DataItem0"" />
      </SeriesDimensions>
      <Arguments>
        <Argument DefaultId=""DataItem1"" />
      </Arguments>
      <Panes>
        <Pane Name=""Pane 1"">
          <AxisY TitleVisible=""false"" />
          <Series>
            <Simple SeriesType=""StackedSplineArea"">
              <Value DefaultId=""DataItem2"" />
            </Simple>
          </Series>
        </Pane>
      </Panes>
      <AxisX EnableZooming=""true"" />
    </Chart>
  </Items>

  <ColorScheme>
    <Entry DataSource=""sqlDataSource1"" DataMember=""workforcevsavailabilityview"" Color=""-11425852"">
      <DimensionKeys>
        <DimensionKey>
          <Definition DataMember=""Name"" />
          <Value Type=""System.String"" Value=""Entrada_Putaway"" />
        </DimensionKey>
      </DimensionKeys>
    </Entry>
    <Entry DataSource=""sqlDataSource1"" DataMember=""workforcevsavailabilityview"" Color=""-2595728"">
      <DimensionKeys>
        <DimensionKey>
          <Definition DataMember=""Name"" />
          <Value Type=""System.String"" Value=""Entrada_Unload"" />
        </DimensionKey>
      </DimensionKeys>
    </Entry>
    <Entry DataSource=""sqlDataSource1"" DataMember=""workforcevsavailabilityview"" Color=""-3944740"">
      <DimensionKeys>
        <DimensionKey>
          <Definition DataMember=""Name"" />
          <Value Type=""System.String"" Value=""Salida_Load"" />
        </DimensionKey>
      </DimensionKeys>
    </Entry>
    <Entry DataSource=""sqlDataSource1"" DataMember=""workforcevsavailabilityview"" Color=""-2380982"">
      <DimensionKeys>
        <DimensionKey>
          <Definition DataMember=""Name"" />
          <Value Type=""System.String"" Value=""Salida_Shipping"" />
        </DimensionKey>
      </DimensionKeys>
    </Entry>
    <Entry DataSource=""sqlDataSource1"" DataMember=""WFMLaborWorkerPerProcessType"" Color=""-11425852"">
      <MeasureKey>
        <Definition DataMember=""WorkTimePerMinutes"" FilterString=""[DataItem1] = [DataItem0]"">
          <Definitions>
            <DimensionDefinition Name=""DataItem1"" DataMember=""CreationDate"" DateTimeGroupInterval=""None"" />
            <DimensionDefinition Name=""DataItem0"" DataMember=""MaxPlanning"" DateTimeGroupInterval=""None"" />
          </Definitions>
        </Definition>
      </MeasureKey>
    </Entry>
    <Entry DataSource=""sqlDataSource1"" DataMember=""WFMLaborWorkerPerProcessType"" Color=""-11425852"">
      <DimensionKeys>
        <DimensionKey>
          <Definition DataMember=""ProcessType"" />
          <Value Type=""System.String"" Value=""Inbound"" />
        </DimensionKey>
      </DimensionKeys>
    </Entry>
    <Entry DataSource=""sqlDataSource1"" DataMember=""WFMLaborWorkerPerProcessType"" Color=""-2595728"">
      <DimensionKeys>
        <DimensionKey>
          <Definition DataMember=""ProcessType"" />
          <Value Type=""System.String"" Value=""Loading"" />
        </DimensionKey>
      </DimensionKeys>
    </Entry>
    <Entry DataSource=""sqlDataSource1"" DataMember=""WFMLaborWorkerPerProcessType"" Color=""-3944740"">
      <DimensionKeys>
        <DimensionKey>
          <Definition DataMember=""ProcessType"" />
          <Value Type=""System.String"" Value=""Putaway"" />
        </DimensionKey>
      </DimensionKeys>
    </Entry>
    <Entry DataSource=""sqlDataSource1"" DataMember=""WFMLaborWorkerPerProcessType"" Color=""-2380982"">
      <DimensionKeys>
        <DimensionKey>
          <Definition DataMember=""ProcessType"" />
          <Value Type=""System.String"" Value=""Shipping"" />
        </DimensionKey>
      </DimensionKeys>
    </Entry>
  </ColorScheme>

  <LayoutTree>
    <LayoutGroup>
      <LayoutItem DashboardItem=""chartDashboardItem1"" Weight=""0.9126317395662673"" />
      <LayoutItem DashboardItem=""chartDashboardItem2"" Weight=""0.9126317395662673"" />
      <LayoutItem DashboardItem=""pivotDashboardItem1"" Weight=""0.9126317395662673"" />
      <LayoutItem DashboardItem=""chartDashboardItem3"" Weight=""0.9126317395662673"" />
    </LayoutGroup>
  </LayoutTree>
</Dashboard>";



    }
}