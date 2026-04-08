namespace Mss.WorkForce.Code.Models.DataAccess
{
    public static class PerformanceMetricsDashBoard
    {

        public static string dashboard = @"<?xml version=""1.0"" encoding=""utf-8""?>
<Dashboard>
  <Title Text=""WFM"" />
  <DataSources>
    <SqlDataSource Name=""PerformanceMetrics"" ComponentName=""sqlDataSource1"">
      <Connection Name=""PerformanceMetrics"" FromAppConfig=""true"" />
      <Query Type=""SelectQuery"" Name=""PerformanceMetrics"">
        <Tables>
          <Table Name=""PerformanceMetrics"" />
        </Tables>
        <Columns>
          <Column Table=""PerformanceMetrics"" Name=""Organization"" />
          <Column Table=""PerformanceMetrics"" Name=""Site"" />
          <Column Table=""PerformanceMetrics"" Name=""When"" />
          <Column Table=""PerformanceMetrics"" Name=""Layout"" />
          <Column Table=""PerformanceMetrics"" Name=""NumberOfWorkers"" />
          <Column Table=""PerformanceMetrics"" Name=""NumberOfOrders"" />
          <Column Table=""PerformanceMetrics"" Name=""NumberOfLines"" />
          <Column Table=""PerformanceMetrics"" Name=""SimulationTime"" />
          <Column Table=""PerformanceMetrics"" Name=""NumberOfSimulations"" />
          <Column Table=""PerformanceMetrics"" Name=""Date"" />
        </Columns>
      </Query>
      <ConnectionOptions CloseConnection=""true"" />
    </SqlDataSource>
  </DataSources>
  <Items>
    <Grid ComponentName=""gridDashboardItem1"" Name="""" DataSource=""sqlDataSource1"" DataMember=""PerformanceMetrics"">
      <DataItems>
        <Dimension DataMember=""Site"" DefaultId=""DataItem1"" />
        <Measure DataMember=""NumberOfWorkers"" DefaultId=""DataItem2"">
          <NumericFormat FormatType=""Number"" />
        </Measure>
        <Measure DataMember=""NumberOfOrders"" DefaultId=""DataItem3"">
          <NumericFormat FormatType=""Number"" />
        </Measure>
        <Measure DataMember=""NumberOfLines"" DefaultId=""DataItem4"">
          <NumericFormat FormatType=""Number"" />
        </Measure>
        <Measure DataMember=""SimulationTime"" DefaultId=""DataItem5"">
          <NumericFormat FormatType=""Number"" Unit=""Ones"" />
        </Measure>
        <Dimension DataMember=""Date"" DateTimeGroupInterval=""DateHourMinute"" DefaultId=""DataItem0"" />
      </DataItems>
      <GridColumns>
        <GridDimensionColumn>
          <Dimension DefaultId=""DataItem0"" />
        </GridDimensionColumn>
        <GridDimensionColumn>
          <Dimension DefaultId=""DataItem1"" />
        </GridDimensionColumn>
        <GridMeasureColumn>
          <Measure DefaultId=""DataItem2"" />
        </GridMeasureColumn>
        <GridMeasureColumn>
          <Measure DefaultId=""DataItem3"" />
        </GridMeasureColumn>
        <GridMeasureColumn>
          <Measure DefaultId=""DataItem4"" />
        </GridMeasureColumn>
        <GridMeasureColumn>
          <Measure DefaultId=""DataItem5"" />
        </GridMeasureColumn>
      </GridColumns>
      <GridOptions />
      <ColumnFilterOptions />
    </Grid>
    <Chart ComponentName=""chartDashboardItem1"" Name="""" DataSource=""sqlDataSource1"" DataMember=""PerformanceMetrics"">
      <DataItems>
        <Measure DataMember=""SimulationTime"" DefaultId=""DataItem0"">
          <NumericFormat FormatType=""Number"" Unit=""Ones"" />
        </Measure>
        <Dimension DataMember=""NumberOfLines"" DefaultId=""DataItem1"">
          <NumericFormat FormatType=""Number"" Precision=""0"" Unit=""Ones"" />
        </Dimension>
      </DataItems>
      <Arguments>
        <Argument DefaultId=""DataItem1"" />
      </Arguments>
      <Panes>
        <Pane Name=""Pane 1"">
          <AxisY TitleVisible=""false"" />
          <Series>
            <Simple Name=""Simulation time (ms)"" SeriesType=""SplineArea"">
              <Value DefaultId=""DataItem0"" />
            </Simple>
          </Series>
        </Pane>
      </Panes>
      <AxisX EnableZooming=""true"" />
    </Chart>
  </Items>
  <LayoutTree>
    <LayoutGroup>
      <LayoutGroup Orientation=""Vertical"">
        <LayoutItem DashboardItem=""gridDashboardItem1"" />
        <LayoutItem DashboardItem=""chartDashboardItem1"" />
      </LayoutGroup>
    </LayoutGroup>
  </LayoutTree>
</Dashboard>";
    }
}
