using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mss.WorkForce.Code.DataBaseManager.Migrations
{
    /// <inheritdoc />
    public partial class workforcevsavailabilityview : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"CREATE OR REPLACE VIEW public.workforcevsavailabilityview
AS WITH plan AS (
         SELECT DISTINCT ""Plannings"".""Id"",
            ""Plannings"".""CreationDate"",
			""Plannings"".""WarehouseId""
           FROM ""Plannings"" 
		   JOIN ""WFMLaborWorkerPerProcessType"" wp ON ""Plannings"".""Id"" = wp.""PlanningId""
          ORDER BY ""Plannings"".""CreationDate"" DESC
         LIMIT 1
        ), itemplanning AS (
         SELECT ip.""Id"",
            ip.""InitDate"",
            ip.""EndDate"",
            wp.""WorkTime"",
			w.""Name"" as workername,
            r.""Name""
           FROM plan p
		   JOIN ""WFMLaborWorkerPerProcessType"" wp ON p.""Id"" = wp.""PlanningId""
             JOIN ""WFMLaborItemPlanning"" ip ON wp.""Id"" = ip.""WFMLaborPerProcessTypeId""
			 JOIN ""Workers"" w ON ip.""WorkerId"" = w.""Id""
			 JOIN ""Roles"" r ON w.""RolId"" = r.""Id""
			), limits AS (
         SELECT date_trunc('day'::text, now()) AS firsthour,
            date_trunc('hour'::text, max(itemplanning.""EndDate"")) + '01:00:00'::interval AS lasthour
           FROM itemplanning
        ), horas AS (
         SELECT generate_series(( SELECT limits.firsthour
                   FROM limits), ( SELECT limits.lasthour + '01:00:00'::interval
                   FROM limits), '01:00:00'::interval) AS hour
        ), intervals AS (
         SELECT DISTINCT itemplanning.""Name"",
		 	itemplanning.workername,
		 	itemplanning.""InitDate"",
            itemplanning.""EndDate""
           FROM itemplanning
        ), intervals_c AS (
         SELECT i.""Name"",
		 	i.workername,
		 	h_1.hour,
            GREATEST(i.""InitDate"", h_1.hour) AS ""InitDate"",
            LEAST(i.""EndDate"", h_1.hour + '01:00:00'::interval) AS ""EndDate""
           FROM intervals i
             JOIN horas h_1 ON i.""InitDate"" < (h_1.hour + '01:00:00'::interval) AND i.""EndDate"" >= h_1.hour
        ), intervals_lag AS (
         SELECT intervals_c.""Name"",
		 	intervals_c.workername,
		 	intervals_c.hour,
            intervals_c.""InitDate"",
            intervals_c.""EndDate"",
            lag(intervals_c.""InitDate"") OVER (PARTITION BY intervals_c.""Name"", intervals_c.hour ORDER BY intervals_c.""InitDate"") AS prev_init_date,
            max(intervals_c.""EndDate"") OVER (PARTITION BY intervals_c.""Name"", intervals_c.hour ORDER BY intervals_c.""InitDate"" ROWS BETWEEN UNBOUNDED PRECEDING AND 1 PRECEDING) AS prev_end_date
           FROM intervals_c
        ), intervals_withgroups AS (
         SELECT intervals_lag.""Name"",
		 	intervals_lag.workername,
		 	intervals_lag.hour,
            intervals_lag.""InitDate"",
            intervals_lag.""EndDate"",
            intervals_lag.prev_init_date,
            intervals_lag.prev_end_date,
                CASE
                    WHEN intervals_lag.prev_end_date IS NULL THEN 1 
                    WHEN intervals_lag.""InitDate"" > intervals_lag.prev_end_date THEN 1
                    ELSE 0
                END AS is_new_group
           FROM intervals_lag
        ), intervals_groups AS (
         SELECT intervals_withgroups.""Name"",
		 	intervals_withgroups.workername,
		 	intervals_withgroups.hour,
            intervals_withgroups.""InitDate"",
            intervals_withgroups.""EndDate"",
			intervals_withgroups.""EndDate"" - intervals_withgroups.""InitDate"" as worktime,
            intervals_withgroups.prev_init_date,
            intervals_withgroups.prev_end_date,
            intervals_withgroups.is_new_group,
            sum(intervals_withgroups.is_new_group) OVER (PARTITION BY intervals_withgroups.""Name"", intervals_withgroups.hour ORDER BY intervals_withgroups.""InitDate"") AS grupo
           FROM intervals_withgroups
        ),intervals_merged AS (
         SELECT intervals_groups.""Name"",
		 	intervals_groups.workername,
		 	intervals_groups.hour,
            min(intervals_groups.""InitDate"") AS ""InitDate"",
            max(intervals_groups.""EndDate"") AS ""EndDate"",
			sum(intervals_groups.worktime) as worktime
           FROM intervals_groups
          GROUP BY intervals_groups.""Name"", intervals_groups.workername, intervals_groups.hour, intervals_groups.grupo
        ), intervals_merged_grouped as (SELECT intervals_merged.""Name"", intervals_merged.hour,
            sum(worktime) AS worktime
           FROM intervals_merged
          GROUP BY intervals_merged.""Name"", intervals_merged.hour),
		workerShifts AS (
  select r.""Name"", sc.""AvailableWorkerId"", sh.""InitHour"", sh.""EndHour"", sh.""WarehouseId"" from public.""Schedules""sc
  join public.""Shifts"" sh  on sc.""ShiftId"" = sh.""Id""
  join public.""AvailableWorkers"" a on sc.""AvailableWorkerId"" = a.""Id""
  join public.""Workers"" w on a.""WorkerId"" = w.""Id""
  JOIN ""Roles"" r ON w.""RolId"" = r.""Id""
        ), workerShiftsForLastPlanning AS (
         SELECT w.""Name"", w.""AvailableWorkerId"", w.""InitHour"", w.""EndHour""
           FROM workerShifts w
		   join plan p
		   on w.""WarehouseId"" = p.""WarehouseId""
        ), horasworkerShifts AS (
		SELECT generate_series(w.""InitHour""::numeric, w.""EndHour""::numeric, 1) as hour, w.""AvailableWorkerId"", w.""Name""
    	FROM workerShiftsForLastPlanning w   
        ), intervals_grouped AS (
         SELECT horasworkerShifts.hour,
		 	horasworkerShifts.""Name"",
		 	count(*)*3600 as availabilitytime
           FROM horasworkerShifts
          GROUP BY horasworkerShifts.""Name"", horasworkerShifts.hour
        )
SELECT h.""Name"",
	h.""When"",
    COALESCE(wt.worktime * 100/h.availabilitytime,0::numeric)	as workforcevsavailability
   FROM (select date_trunc('day'::text, now()) + hour * INTERVAL '1 hour' AS ""When"", ""Name"",
    availabilitytime from intervals_grouped) h
   left JOIN (select a.""Name"", h.hour AS ""When"",
   COALESCE(EXTRACT(epoch FROM a.worktime), 0::numeric) AS worktime
   FROM horas h
     LEFT JOIN intervals_merged_grouped a ON h.hour = a.hour)wt
	on h.""When""= wt.""When"" and h.""Name""= wt.""Name""
  ORDER BY h.""Name"", h.""When"";");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"DROP VIEW IF EXISTS public.workforcevsavailabilityview ;");

        }
    }
}
