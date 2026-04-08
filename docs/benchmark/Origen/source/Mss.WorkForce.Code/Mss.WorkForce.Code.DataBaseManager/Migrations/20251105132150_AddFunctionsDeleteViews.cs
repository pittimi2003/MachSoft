using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mss.WorkForce.Code.DataBaseManager.Migrations
{
    /// <inheritdoc />
    public partial class AddFunctionsDeleteViews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP VIEW IF EXISTS public.areaview;");
            migrationBuilder.Sql("DROP VIEW IF EXISTS public.planningresourcesview;");
            migrationBuilder.Sql("DROP VIEW IF EXISTS public.processinterleavingview;");
            migrationBuilder.Sql("DROP VIEW IF EXISTS public.resourcesusageview;");
            migrationBuilder.Sql("DROP VIEW IF EXISTS public.workforcevsavailabilityview;");

            //Workforce Availability Function
            migrationBuilder.Sql(@"CREATE OR REPLACE FUNCTION public.workforcevsavailabilityview_p(_planning_id uuid, _utc_offset interval DEFAULT '00:00:00'::interval, _hour_fmt text DEFAULT 'HH24:MI'::text)
             RETURNS TABLE(""Name"" text, ""When"" timestamp without time zone, workforcevsavailability numeric, when_utc_txt text)
             LANGUAGE sql
             STABLE
            AS $function$
            WITH plan AS (
              SELECT p.""Id"", p.""CreationDate"", p.""WarehouseId""
              FROM public.""Plannings"" p
              WHERE p.""Id"" = _planning_id
            ),
            itemplanning AS (
              SELECT ip.""Id"",
                     ip.""InitDate"",
                     ip.""EndDate"",
                     wp.""WorkTime"",
                     w.""Name"" AS workername,
                     r.""Name""
              FROM plan p
              JOIN public.""WFMLaborWorkerPerProcessType"" wp ON p.""Id"" = wp.""PlanningId""
              JOIN public.""WFMLaborItemPlanning"" ip       ON wp.""Id"" = ip.""WFMLaborPerProcessTypeId""
              JOIN public.""Workers"" w                     ON ip.""WorkerId"" = w.""Id""
              JOIN public.""Roles""   r                     ON w.""RolId"" = r.""Id""
            ),
            limits AS (
              SELECT 
                date_trunc('day', now()) AS firsthour,
                date_trunc('hour', max(itemplanning.""EndDate"")) + interval '1 hour' AS lasthour
              FROM itemplanning
            ),
            horas AS (
              SELECT generate_series(
                       (SELECT firsthour FROM limits),
                       (SELECT lasthour  FROM limits) + interval '1 hour',
                       interval '1 hour'
                     ) AS hour
            ),
            intervals AS (
              SELECT DISTINCT
                     itemplanning.""Name"",
                     itemplanning.workername,
                     itemplanning.""InitDate"",
                     itemplanning.""EndDate""
              FROM itemplanning
            ),
            intervals_c AS (
              SELECT i.""Name"",
                     i.workername,
                     h.hour,
                     GREATEST(i.""InitDate"", h.hour)                    AS ""InitDate"",
                     LEAST(i.""EndDate"",   h.hour + interval '1 hour')  AS ""EndDate""
              FROM intervals i
              JOIN horas h
                ON i.""InitDate"" < (h.hour + interval '1 hour')
               AND i.""EndDate""  >= h.hour
            ),
            intervals_lag AS (
              SELECT ic.""Name"",
                     ic.workername,
                     ic.hour,
                     ic.""InitDate"",
                     ic.""EndDate"",
                     lag(ic.""InitDate"") OVER (PARTITION BY ic.""Name"", ic.hour ORDER BY ic.""InitDate"") AS prev_init_date,
                     max(ic.""EndDate"")  OVER (PARTITION BY ic.""Name"", ic.hour ORDER BY ic.""InitDate"" ROWS BETWEEN UNBOUNDED PRECEDING AND 1 PRECEDING) AS prev_end_date
              FROM intervals_c ic
            ),
            intervals_withgroups AS (
              SELECT il.""Name"",
                     il.workername,
                     il.hour,
                     il.""InitDate"",
                     il.""EndDate"",
                     il.prev_init_date,
                     il.prev_end_date,
                     CASE WHEN il.prev_end_date IS NULL THEN 1
                          WHEN il.""InitDate"" > il.prev_end_date THEN 1
                          ELSE 0 END AS is_new_group
              FROM intervals_lag il
            ),
            intervals_groups AS (
              SELECT iwg.""Name"",
                     iwg.workername,
                     iwg.hour,
                     iwg.""InitDate"",
                     iwg.""EndDate"",
                     (iwg.""EndDate"" - iwg.""InitDate"") AS worktime,
                     iwg.prev_init_date,
                     iwg.prev_end_date,
                     iwg.is_new_group,
                     sum(iwg.is_new_group) OVER (PARTITION BY iwg.""Name"", iwg.hour ORDER BY iwg.""InitDate"") AS grupo
              FROM intervals_withgroups iwg
            ),
            intervals_merged AS (
              SELECT ig.""Name"",
                     ig.workername,
                     ig.hour,
                     min(ig.""InitDate"") AS ""InitDate"",
                     max(ig.""EndDate"")  AS ""EndDate"",
                     sum(ig.worktime)   AS worktime
              FROM intervals_groups ig
              GROUP BY ig.""Name"", ig.workername, ig.hour, ig.grupo
            ),
            intervals_merged_grouped AS (
              SELECT im.""Name"",
                     date_part('hour', im.hour)::int AS hour_int,
                     sum(im.worktime) AS worktime
              FROM (
                SELECT im.""Name"",
                       date_trunc('hour', im.hour) AS hour
                FROM intervals_merged im
              ) t
              JOIN intervals_merged im ON im.""Name"" = t.""Name"" AND im.hour = t.hour
              GROUP BY im.""Name"", date_part('hour', im.hour)
            ),
            workershifts AS (
              SELECT r.""Name"",
                     sc.""AvailableWorkerId"",
                     sh.""InitHour"",
                     sh.""EndHour"",
                     sh.""WarehouseId""
              FROM public.""Schedules"" sc
              JOIN public.""Shifts""    sh ON sc.""ShiftId"" = sh.""Id""
              JOIN public.""AvailableWorkers"" a ON sc.""AvailableWorkerId"" = a.""Id""
              JOIN public.""Workers""   w ON a.""WorkerId"" = w.""Id""
              JOIN public.""Roles""     r ON w.""RolId"" = r.""Id""
            ),
            workershiftsforlastplanning AS (
              SELECT w.""Name"",
                     w.""AvailableWorkerId"",
                     w.""InitHour"",
                     w.""EndHour"",
                     p.""WarehouseId""
              FROM workershifts w
              JOIN plan p ON w.""WarehouseId"" = p.""WarehouseId""
            ),
            horasworkershifts AS (
              SELECT generate_series(
                       CASE
                         WHEN date_part('hour', min(ip.""InitDate"")) > w.""InitHour"" THEN floor(w.""InitHour"")::int
                         ELSE floor(date_part('hour', min(ip.""InitDate"")))::int
                       END,
                       CASE
                         WHEN date_part('hour', max(ip.""EndDate"")) > w.""EndHour"" THEN ceiling(date_part('hour', max(ip.""EndDate"")))::int
                         ELSE ceiling(w.""EndHour"")::int
                       END, 1
                     ) AS hour_int,
                     w.""AvailableWorkerId"",
                     w.""Name""
              FROM workershiftsforlastplanning w, itemplanning ip
              GROUP BY w.""AvailableWorkerId"", w.""Name"", w.""InitHour"", w.""EndHour""
            ),
            intervals_grouped AS (
              SELECT hw.hour_int,
                     hw.""Name"",
                     count(*) * 3600 AS availabilitytime
              FROM horasworkershifts hw
              GROUP BY hw.""Name"", hw.hour_int
            ),
            serie_utc AS (
              SELECT ig.""Name"",
                     (date_trunc('day', now()) + (ig.hour_int * interval '1 hour'))::timestamptz AS when_utc,
                     ig.availabilitytime
              FROM intervals_grouped ig
            ),
            worktime_utc AS (
              SELECT img.""Name"",
                     (date_trunc('day', now()) + (img.hour_int * interval '1 hour'))::timestamptz AS when_utc,
                     COALESCE(EXTRACT(epoch FROM img.worktime), 0::numeric) AS worktime
              FROM intervals_merged_grouped img
            )
            ,
            base AS (
              SELECT s.""Name"",
                     s.when_utc,
                     s.availabilitytime,
                     COALESCE(wt.worktime, 0::numeric) AS worktime
              FROM serie_utc s
              LEFT JOIN worktime_utc wt
                ON s.""Name"" = wt.""Name""
               AND s.when_utc = wt.when_utc
            )
            SELECT
              b.""Name"",
              ((b.when_utc AT TIME ZONE 'UTC') + _utc_offset) AS ""When"",
              COALESCE(b.worktime * 100::numeric / NULLIF(b.availabilitytime::numeric, 0), 0::numeric) AS workforcevsavailability,
              to_char(date_trunc('hour', (b.when_utc AT TIME ZONE 'UTC') + _utc_offset), _hour_fmt) AS when_utc_txt
            FROM base b
            ORDER BY b.""Name"", ""When"";
            $function$
            ;");

            //Resources Usage Function
            migrationBuilder.Sql(@"CREATE OR REPLACE FUNCTION public.resourcesusageview_p(_planning_id uuid, _utc_offset interval DEFAULT '00:00:00'::interval, _hour_fmt text DEFAULT 'HH24:MI'::text)
             RETURNS TABLE(""When"" timestamp without time zone, ""ResourceName"" text, ""ResourcesUt"" numeric, when_utc_txt text)
             LANGUAGE sql
             STABLE
            AS $function$
            WITH plan AS (
              SELECT p.""Id"", p.""CreationDate""
              FROM public.""Plannings"" p
              WHERE p.""Id"" = _planning_id
            ),
            itemplanning AS (
              SELECT
                ip.""Id"",
                ip.""InitDate"",
                ip.""EndDate"",
                ip.""WorkTime"",
                ip.""IsStored"",
                ip.""IsBlocked"",
                ip.""IsStarted"",
                ip.""WorkOrderPlanningId"",
                ip.""WorkerId"",
                ro.""Name"" AS ""ResourceName""
              FROM plan p
              JOIN public.""WorkOrdersPlanning"" wo ON p.""Id"" = wo.""PlanningId""
              JOIN public.""ItemsPlanning"" ip      ON wo.""Id"" = ip.""WorkOrderPlanningId""
              JOIN public.""Processes"" po          ON ip.""ProcessId"" = po.""Id""
              JOIN public.""Areas"" a               ON po.""AreaId"" = a.""Id""
              LEFT JOIN public.""Workers"" w        ON ip.""WorkerId"" = w.""Id""
              FULL JOIN public.""Roles""   ro       ON w.""RolId""   = ro.""Id""
            ),
            limits AS (
              SELECT
                date_trunc('day', now()) AS firsthour,
                date_trunc('hour', max(itemplanning.""EndDate"")) + interval '1 hour' AS lasthour
              FROM itemplanning
            ),
            horas AS (
              SELECT generate_series(
                       (SELECT firsthour FROM limits),
                       (SELECT lasthour  FROM limits),
                       interval '1 hour'
                     ) AS hora  
            ),
            intervals AS (
              SELECT DISTINCT
                itemplanning.""InitDate"",
                itemplanning.""EndDate"",
                itemplanning.""ResourceName""
              FROM itemplanning
            ),
            cortes AS (
              SELECT
                h.hora,
                i.""ResourceName"",
                GREATEST(i.""InitDate"", h.hora)                   AS ""InitDate"",
                LEAST(i.""EndDate"",   h.hora + interval '1 hour') AS ""EndDate""
              FROM intervals i
              JOIN horas h
                ON i.""InitDate"" < (h.hora + interval '1 hour')
               AND i.""EndDate""  >= h.hora
            ),
            groups AS (
              SELECT
                sub.""ResourceName"",
                sub.""InitDate"",
                sub.""EndDate"",
                sum(sub.change) OVER (PARTITION BY sub.""ResourceName"" ORDER BY sub.""InitDate"") AS group_id
              FROM (
                SELECT
                  c.hora,
                  c.""ResourceName"",
                  c.""InitDate"",
                  c.""EndDate"",
                  CASE
                    WHEN c.""InitDate"" >
                         COALESCE(lag(c.""EndDate"") OVER (PARTITION BY c.""ResourceName"" ORDER BY c.""InitDate""), c.""InitDate"")
                    THEN 1 ELSE 0
                  END AS change
                FROM cortes c
              ) sub
            ),
            grouped_intervals AS (
              SELECT
                g.""ResourceName"",
                min(g.""InitDate"") AS ""InitDate"",
                max(g.""EndDate"")  AS ""EndDate""
              FROM groups g
              GROUP BY g.""ResourceName"", g.group_id
            ),
            final_intervals AS (
              SELECT
                sub.""ResourceName"",
                sub.truncated_date AS hour,
                GREATEST(sub.""InitDate"", sub.truncated_date)                   AS ""InitDate"",
                LEAST(sub.""EndDate"",   sub.truncated_date + interval '1 hour') AS ""EndDate""
              FROM (
                SELECT
                  gi.""ResourceName"",
                  gi.""InitDate"",
                  gi.""EndDate"",
                  generate_series(
                    date_trunc('hour', gi.""InitDate""),
                    date_trunc('hour', gi.""EndDate""),
                    interval '1 hour'
                  ) AS truncated_date
                FROM grouped_intervals gi
              ) sub
            ),
            unified_groups AS (
              SELECT
                fi.""ResourceName"",
                fi.hour AS ""When"",
                fi.""InitDate"",
                fi.""EndDate"",
                sum(sub.change) OVER (
                  PARTITION BY fi.""ResourceName""
                  ORDER BY fi.""InitDate"", fi.""EndDate""
                ) AS group_id
              FROM final_intervals fi
              JOIN (
                SELECT
                  fi2.""ResourceName"",
                  fi2.""InitDate"",
                  fi2.""EndDate"",
                  CASE
                    WHEN fi2.""InitDate"" >
                         COALESCE(lag(fi2.""EndDate"") OVER (PARTITION BY fi2.""ResourceName"" ORDER BY fi2.""InitDate"", fi2.""EndDate""), fi2.""InitDate"")
                    THEN 1 ELSE 0
                  END AS change
                FROM final_intervals fi2
              ) sub
                ON sub.""ResourceName"" = fi.""ResourceName""
               AND sub.""InitDate""     = fi.""InitDate""
               AND sub.""EndDate""      = fi.""EndDate""
            ),
            final_unified_intervals AS (
              SELECT
                ug.""When"",
                min(ug.""InitDate"") AS ""InitDate"",
                max(ug.""EndDate"")  AS ""EndDate"",
                ug.""ResourceName""
              FROM unified_groups ug
              GROUP BY ug.""ResourceName"", ug.""When"", ug.group_id
            ),
            agg AS (
              SELECT
                fui.""When""::timestamptz AS when_utc,
                fui.""ResourceName"",
                sum(EXTRACT(epoch FROM (fui.""EndDate"" - fui.""InitDate""))) / 3600::numeric AS ""ResourcesUt""
              FROM final_unified_intervals fui
              GROUP BY fui.""When"", fui.""ResourceName""
            )
            SELECT
              ( (h.hora AT TIME ZONE 'UTC') + _utc_offset ) AS ""When"",
              a.""ResourceName"",
              COALESCE(ag.""ResourcesUt"", 0::numeric) AS ""ResourcesUt"",
              to_char(date_trunc('hour', (h.hora AT TIME ZONE 'UTC') + _utc_offset), _hour_fmt) AS when_utc_txt
            FROM horas h
            CROSS JOIN (SELECT DISTINCT r.""Name"" AS ""ResourceName"" FROM public.""Roles"" r) a
            LEFT JOIN agg ag
              ON h.hora = ag.when_utc
             AND a.""ResourceName"" = ag.""ResourceName""
            ORDER BY 1, a.""ResourceName"";

            $function$
            ;");

            //Process Interleaving Function
            migrationBuilder.Sql(@"CREATE OR REPLACE FUNCTION public.processinterleavingview_p(_planning_id uuid, _utc_offset text, _hour_format text)
             RETURNS TABLE(""When"" timestamp without time zone, ""InboundSec"" numeric, ""OutboundSec"" numeric, ""InterleavingSec"" numeric, ""NoWorkingSec"" numeric)
             LANGUAGE sql
             STABLE
            AS $function$
            WITH plan AS (
              SELECT p.""Id""
              FROM public.""Plannings"" p
              WHERE p.""Id"" = _planning_id
            ),
            itemplanning AS (
              SELECT
                ip.""IsOutbound"",
                ip.""InitDate""::timestamptz AS ""InitUTC"",
                ip.""EndDate""::timestamptz  AS ""EndUTC""
              FROM plan p
              JOIN public.""WorkOrdersPlanning"" wo ON p.""Id"" = wo.""PlanningId""
              JOIN public.""ItemsPlanning"" ip      ON wo.""Id"" = ip.""WorkOrderPlanningId""
            ),
            ip_local AS (
              SELECT
                ""IsOutbound"",
            (""InitUTC"" AT TIME ZONE 'UTC' + (_utc_offset::interval)) AS ""InitLocal"",
            (""EndUTC""  AT TIME ZONE 'UTC' + (_utc_offset::interval)) AS ""EndLocal""
              FROM itemplanning
            ),
            limits AS (
              SELECT
                date_trunc('day', min(""InitLocal""))                         AS firsthour_local,
                date_trunc('hour', max(""EndLocal"")) + interval '1 hour'     AS lasthour_local
              FROM ip_local
            ),
            horas_local AS (
              SELECT generate_series(
                       (SELECT firsthour_local FROM limits),
                       (SELECT lasthour_local  FROM limits),
                       interval '1 hour'
                     ) AS hora_local
            ),
            intervals AS (
              SELECT DISTINCT
                l.""InitLocal"" AS ""InitLocal"",
                l.""EndLocal""  AS ""EndLocal"",
                l.""IsOutbound""
              FROM ip_local l
            ),
            intervals_c AS (
              SELECT
                h.hora_local,
                i.""IsOutbound"",
                GREATEST(i.""InitLocal"", h.hora_local)                   AS ""InitLocal"",
                LEAST(i.""EndLocal"",  h.hora_local + interval '1 hour')  AS ""EndLocal""
              FROM intervals i
              JOIN horas_local h
                ON i.""InitLocal"" < (h.hora_local + interval '1 hour')
               AND i.""EndLocal""  >= h.hora_local
            ),
            intervals_dup AS (
              SELECT DISTINCT second.hora_local,
                              second.""IsOutbound"",
                              second.""InitLocal"",
                              second.""EndLocal""
              FROM (
                SELECT first.*,
                       row_number() OVER (
                         PARTITION BY first.""IsOutbound"", first.hora_local, first.""EndLocal""
                         ORDER BY first.""InitLocal""
                       ) AS rn2
                FROM (
                  SELECT ic.*,
                         row_number() OVER (
                           PARTITION BY ic.""IsOutbound"", ic.hora_local, ic.""InitLocal""
                           ORDER BY ic.""EndLocal"" DESC
                         ) AS rn1
                  FROM intervals_c ic
                ) first
                WHERE first.rn1 = 1
              ) second
              WHERE second.rn2 = 1
            ),
            intervals_lag AS (
              SELECT
                d.hora_local,
                d.""IsOutbound"",
                d.""InitLocal"",
                d.""EndLocal"",
                lag(d.""InitLocal"") OVER (PARTITION BY d.hora_local, d.""IsOutbound"" ORDER BY d.""InitLocal"") AS prev_init,
                max(d.""EndLocal"")  OVER (PARTITION BY d.hora_local, d.""IsOutbound"" ORDER BY d.""InitLocal"" ROWS BETWEEN UNBOUNDED PRECEDING AND 1 PRECEDING) AS prev_end
              FROM intervals_dup d
            ),
            intervals_withgroups AS (
              SELECT
                l.hora_local,
                l.""IsOutbound"",
                l.""InitLocal"",
                l.""EndLocal"",
                CASE
                  WHEN l.prev_end IS NULL THEN 1
                  WHEN l.""InitLocal"" > l.prev_end THEN 1
                  ELSE 0
                END AS is_new_group
              FROM intervals_lag l
            ),
            intervals_groups AS (
              SELECT
                w.hora_local,
                w.""IsOutbound"",
                w.""InitLocal"",
                w.""EndLocal"",
                sum(w.is_new_group) OVER (PARTITION BY w.hora_local, w.""IsOutbound"" ORDER BY w.""InitLocal"") AS grupo
              FROM intervals_withgroups w
            ),
            intervals_merged AS (
              SELECT
                g.hora_local,
                g.""IsOutbound"",
                min(g.""InitLocal"") AS ""InitLocal"",
                max(g.""EndLocal"")  AS ""EndLocal""
              FROM intervals_groups g
              GROUP BY g.hora_local, g.""IsOutbound"", g.grupo
            ),
            combined AS (
              SELECT
                t.hora_local,
                GREATEST(t.""InitLocal"", f.""InitLocal"") AS ""InitLocal"",
                LEAST(t.""EndLocal"",   f.""EndLocal"")    AS ""EndLocal""
              FROM intervals_merged t
              JOIN intervals_merged f
                ON t.""IsOutbound"" = true
               AND f.""IsOutbound"" = false
               AND t.""InitLocal"" < f.""EndLocal""
               AND t.""EndLocal""  > f.""InitLocal""
            ),
            a_out AS (
              SELECT hora_local,
                     sum(CASE WHEN ""IsOutbound"" THEN EXTRACT(epoch FROM ""EndLocal"" - ""InitLocal"") ELSE 0 END) AS outbound_sec
              FROM intervals_merged
              GROUP BY hora_local
            ),
            b_in AS (
              SELECT hora_local,
                     sum(CASE WHEN NOT ""IsOutbound"" THEN EXTRACT(epoch FROM ""EndLocal"" - ""InitLocal"") ELSE 0 END) AS inbound_sec
              FROM intervals_merged
              GROUP BY hora_local
            ),
            c_inter AS (
              SELECT hora_local,
                     sum(EXTRACT(epoch FROM ""EndLocal"" - ""InitLocal"")) AS interleaving_sec
              FROM combined
              GROUP BY hora_local
            )
            SELECT
              h.hora_local AS ""When"",  
              (COALESCE(b_in.inbound_sec, 0)   - COALESCE(c_inter.interleaving_sec, 0)) / 3600::numeric AS ""InboundSec"",
              (COALESCE(a_out.outbound_sec, 0) - COALESCE(c_inter.interleaving_sec, 0)) / 3600::numeric AS ""OutboundSec"",
              COALESCE(c_inter.interleaving_sec, 0) / 3600::numeric                                   AS ""InterleavingSec"",
              (3600::numeric - (
                  COALESCE(a_out.outbound_sec, 0) - COALESCE(c_inter.interleaving_sec, 0)
                + COALESCE(b_in.inbound_sec, 0)   - COALESCE(c_inter.interleaving_sec, 0)
                + COALESCE(c_inter.interleaving_sec, 0)
              )) / 3600::numeric AS ""NoWorkingSec""
            FROM horas_local h
            LEFT JOIN a_out    ON a_out.hora_local    = h.hora_local
            LEFT JOIN b_in     ON b_in.hora_local     = h.hora_local
            LEFT JOIN c_inter  ON c_inter.hora_local  = h.hora_local
            ORDER BY ""When"";
            $function$
            ;");

            //Planning Resources Function
            migrationBuilder.Sql(@"CREATE OR REPLACE FUNCTION public.planningresourcesview_p(
              _planning_id uuid,
              _utc_offset text,
              _hour_format text
            )
            RETURNS TABLE(
              ""When"" timestamp without time zone,
              ""InitDate"" timestamp without time zone,
              ""EndDate"" timestamp without time zone,
              ""ProcessId"" uuid,
              ""ProcessName"" text,
              ""ResourceType"" text,
              ""ResourceId"" uuid,
              ""ResourceName"" text,
              ""AvailableResources"" integer
            )
            LANGUAGE sql
            STABLE
            AS $function$
            WITH plan AS (
              SELECT p.""Id""
              FROM public.""Plannings"" p
              WHERE p.""Id"" = _planning_id
            ),

            itemplanning_utc AS (
              SELECT
                ip.""Id"",
                ip.""ProcessId"",
                ip.""IsOutbound"",
                ip.""LimitDate""::timestamptz              AS ""LimitUTC"",
                ip.""InitDate""::timestamptz               AS ""InitUTC"",
                ip.""EndDate"" ::timestamptz               AS ""EndUTC"",
                ip.""WorkTime"",
                ip.""IsStored"",
                ip.""IsBlocked"",
                ip.""IsStarted"",
                ip.""WorkOrderPlanningId"",
                ip.""WorkerId"",
                ip.""EquipmentGroupId""
              FROM plan pl
              JOIN public.""WorkOrdersPlanning"" wo ON pl.""Id"" = wo.""PlanningId""
              JOIN public.""ItemsPlanning"" ip      ON wo.""Id"" = ip.""WorkOrderPlanningId""
            ),

            ip_local AS (
              SELECT
                i.""Id"",
                i.""ProcessId"",
                i.""IsOutbound"",
                ((i.""InitUTC"" AT TIME ZONE 'UTC') + (_utc_offset::interval))::timestamp AS ""InitLocal"",
                ((i.""EndUTC""  AT TIME ZONE 'UTC') + (_utc_offset::interval))::timestamp AS ""EndLocal"",
                i.""WorkTime"",
                i.""IsStored"",
                i.""IsBlocked"",
                i.""IsStarted"",
                i.""WorkOrderPlanningId"",
                i.""WorkerId"",
                i.""EquipmentGroupId""
              FROM itemplanning_utc i
            ),

            wpp_utc AS (
              SELECT
                w.""ProcessId"",
                w.""EquipmentGroupId"",
                w.""WorkerId"",
                w.""InitDate""::timestamptz AS ""InitUTC"",
                w.""EndDate"" ::timestamptz AS ""EndUTC""
              FROM public.""WarehouseProcessPlanning"" w
            ),

            wpp_local AS (
              SELECT
                ((w.""InitUTC"" AT TIME ZONE 'UTC') + (_utc_offset::interval))::timestamp AS ""InitLocal"",
                ((w.""EndUTC""  AT TIME ZONE 'UTC') + (_utc_offset::interval))::timestamp AS ""EndLocal"",
                w.""ProcessId"",
                w.""EquipmentGroupId"",
                w.""WorkerId""
              FROM wpp_utc w
            ),

            limits AS (
              SELECT
                date_trunc('day', min(x.""InitLocal""))                     AS firsthour_local,
                date_trunc('hour', max(x.""EndLocal"")) + interval '1 hour' AS lasthour_local
              FROM (
                SELECT ""InitLocal"",""EndLocal"" FROM ip_local
                UNION ALL
                SELECT ""InitLocal"",""EndLocal"" FROM wpp_local
              ) x
            ),
            hours AS (
              SELECT generate_series(
                       (SELECT firsthour_local FROM limits),
                       (SELECT lasthour_local  FROM limits),
                       interval '1 hour'
                     ) AS hora_local
            ),

            worksbyhour AS (
              SELECT h.hora_local AS hora,
                     i.""InitLocal"" AS ""InitDate"",
                     i.""EndLocal""  AS ""EndDate"",
                     i.""ProcessId"",
                     i.""EquipmentGroupId"",
                     i.""WorkerId"",
                     'Operator'::text AS ""ResourceType""
              FROM hours h
              LEFT JOIN ip_local i
                ON i.""InitLocal"" < (h.hora_local + interval '1 hour')
               AND i.""EndLocal""  >  h.hora_local
            ),

            warehousebyhour AS (
              SELECT h.hora_local AS hora,
                     w.""InitLocal"" AS ""InitDate"",
                     w.""EndLocal""  AS ""EndDate"",
                     w.""ProcessId"",
                     w.""EquipmentGroupId"",
                     w.""WorkerId"",
                     'Equipment'::text AS ""ResourceType""
              FROM hours h
              LEFT JOIN wpp_local w
                ON w.""InitLocal"" < (h.hora_local + interval '1 hour')
               AND w.""EndLocal""  >  h.hora_local
            ),

            usedbyhour AS (
              SELECT * FROM warehousebyhour
              UNION ALL
              SELECT * FROM worksbyhour
            ),

            workersbyrol AS (
              SELECT w.""RolId"", count(DISTINCT aw.""WorkerId"") AS ""AvailableWorkers""
              FROM public.""Workers"" w
              JOIN public.""AvailableWorkers"" aw ON w.""Id"" = aw.""WorkerId""
              GROUP BY w.""RolId""
            ),

            allinformation AS (
              SELECT
                u.hora                       AS ""When"",
                u.""InitDate"",
                u.""EndDate"",
                u.""ProcessId"",
                pc.""Name""                    AS ""ProcessName"",
                u.""ResourceType"",
                CASE
                  WHEN u.""ResourceType"" = 'Operator'  THEN u.""WorkerId""
                  ELSE eg.""TypeEquipmentId""
                END                          AS ""ResourceId"",
                CASE
                  WHEN u.""ResourceType"" = 'Operator'  THEN ro.""Name""
                  ELSE eg.""Name""
                END                          AS ""ResourceName"",
                CASE
                  WHEN u.""ResourceType"" = 'Operator'  THEN wr.""AvailableWorkers""
                  ELSE eg.""Equipments""
                END                          AS ""AvailableResources""
              FROM usedbyhour u
              LEFT JOIN public.""Workers"" w          ON u.""WorkerId"" = w.""Id""
              LEFT JOIN public.""Roles""   ro         ON w.""RolId""   = ro.""Id""
              LEFT JOIN public.""Processes"" pc       ON u.""ProcessId"" = pc.""Id""
              LEFT JOIN public.""EquipmentGroups"" eg ON pc.""AreaId"" = eg.""AreaId"" AND eg.""Id"" = u.""EquipmentGroupId""
              LEFT JOIN workersbyrol wr             ON w.""RolId"" = wr.""RolId""
            ),

            resourcesandprocess AS (
              SELECT DISTINCT
                ai.""When"",
                p.""Name""  AS ""ProcessName"",
                r.""Name""  AS ""ResourceName""
              FROM allinformation ai
              CROSS JOIN public.""Processes"" p
              CROSS JOIN public.""Roles""     r
            ),

            availableresourcesnulls AS (
              SELECT
                ai.""ProcessName"",
                ai.""ResourceName"",
                COALESCE(max(ai.""AvailableResources""), 0)::bigint AS ""AvailableResources""
              FROM allinformation ai
              GROUP BY ai.""ProcessName"", ai.""ResourceName""
            )

            SELECT
              r.""When""::timestamp                     AS ""When"",
              a.""InitDate""::timestamp                 AS ""InitDate"",
              a.""EndDate""::timestamp                  AS ""EndDate"",
              a.""ProcessId"",
              r.""ProcessName"",
              a.""ResourceType"",
              a.""ResourceId"",
              r.""ResourceName"",
              COALESCE(a.""AvailableResources"", an.""AvailableResources"", 0)::int AS ""AvailableResources""
            FROM resourcesandprocess r
            LEFT JOIN allinformation a
              ON r.""When""        = a.""When""
             AND r.""ProcessName"" = a.""ProcessName""
             AND r.""ResourceName""= a.""ResourceName""
            LEFT JOIN availableresourcesnulls an
              ON r.""ProcessName"" = an.""ProcessName""
             AND r.""ResourceName""= an.""ResourceName""
            WHERE r.""When"" IS NOT NULL
            ORDER BY r.""When"", r.""ProcessName"", r.""ResourceName"";
            $function$;
            ");

            //Area Function
            migrationBuilder.Sql(@"CREATE OR REPLACE FUNCTION public.areaview_p(_planning_id uuid, _utc_offset text, _hour_format text)
             RETURNS TABLE(""When"" timestamp without time zone, ""AreaName"" text, ""AreasPercUt"" numeric)
             LANGUAGE sql
             STABLE
            AS $function$
            WITH plan AS (
              SELECT p.""Id"", p.""CreationDate""
              FROM public.""Plannings"" p
              WHERE p.""Id"" = _planning_id
            ),
            itemplanning AS (
              SELECT
                ip.""Id"",
                a.""Name"" AS ""AreaName"",
                ip.""InitDate""::timestamptz AS ""InitDateUTC"",
                ip.""EndDate""::timestamptz  AS ""EndDateUTC""
              FROM plan p
              JOIN public.""WorkOrdersPlanning"" wo ON p.""Id"" = wo.""PlanningId""
              JOIN public.""ItemsPlanning"" ip      ON wo.""Id"" = ip.""WorkOrderPlanningId""
              JOIN public.""Processes"" po          ON ip.""ProcessId"" = po.""Id""
              JOIN public.""Areas"" a               ON po.""AreaId"" = a.""Id""
            ),
            ip_local AS (
              SELECT
                ""AreaName"",
                (""InitDateUTC"" + (_utc_offset::interval))::timestamp AS ""InitLocal"",
                (""EndDateUTC""  + (_utc_offset::interval))::timestamp AS ""EndLocal""
              FROM itemplanning
            ),
            limits AS (
              SELECT
                date_trunc('day', min(""InitLocal"")) AS firsthour_local,
                date_trunc('hour', max(""EndLocal"")) + interval '1 hour' AS lasthour_local
              FROM ip_local
            ),
            horas_local AS (
              SELECT generate_series(
                       (SELECT firsthour_local FROM limits),
                       (SELECT lasthour_local  FROM limits),
                       interval '1 hour'
                     ) AS hora_local
            ),
            intervals AS (
              SELECT DISTINCT
                l.""AreaName"",
                l.""InitLocal"",
                l.""EndLocal""
              FROM ip_local l
            ),
            cortes AS (
              SELECT
                h.hora_local,
                i.""AreaName"",
                GREATEST(i.""InitLocal"", h.hora_local)                      AS ""InitLocal"",
                LEAST(i.""EndLocal"",  h.hora_local + interval '1 hour')     AS ""EndLocal""
              FROM intervals i
              JOIN horas_local h
                ON i.""InitLocal"" < (h.hora_local + interval '1 hour')
               AND i.""EndLocal""  >= h.hora_local
            ),
            groups AS (
              SELECT
                sub.""AreaName"",
                sub.""InitLocal"",
                sub.""EndLocal"",
                sum(sub.change) OVER (PARTITION BY sub.""AreaName"" ORDER BY sub.""InitLocal"") AS group_id
              FROM (
                SELECT
                  c.hora_local,
                  c.""AreaName"",
                  c.""InitLocal"",
                  c.""EndLocal"",
                  CASE
                    WHEN c.""InitLocal"" > COALESCE(lag(c.""EndLocal"") OVER (PARTITION BY c.""AreaName"" ORDER BY c.""InitLocal""), c.""InitLocal"")
                      THEN 1 ELSE 0
                  END AS change
                FROM cortes c
              ) sub
            ),
            grouped_intervals AS (
              SELECT
                g.""AreaName"",
                min(g.""InitLocal"") AS ""InitLocal"",
                max(g.""EndLocal"")  AS ""EndLocal""
              FROM groups g
              GROUP BY g.""AreaName"", g.group_id
            ),
            final_intervals AS (
              SELECT
                sub.""AreaName"",
                sub.truncated_date AS hour_local,
                GREATEST(sub.""InitLocal"", sub.truncated_date)                    AS ""InitLocal"",
                LEAST(sub.""EndLocal"",   sub.truncated_date + interval '1 hour')  AS ""EndLocal""
              FROM (
                SELECT
                  gi.""AreaName"",
                  gi.""InitLocal"",
                  gi.""EndLocal"",
                  generate_series(
                    date_trunc('hour', gi.""InitLocal""),
                    date_trunc('hour', gi.""EndLocal""),
                    interval '1 hour'
                  ) AS truncated_date
                FROM grouped_intervals gi
              ) sub
            ),
            unified_groups AS (
              SELECT
                fi.""AreaName"",
                fi.hour_local AS ""WhenLocal"",
                fi.""InitLocal"",
                fi.""EndLocal"",
                sum(sub.change) OVER (PARTITION BY fi.""AreaName"" ORDER BY fi.""InitLocal"", fi.""EndLocal"") AS group_id
              FROM final_intervals fi
              JOIN (
                SELECT
                  fi2.""AreaName"",
                  fi2.""InitLocal"",
                  fi2.""EndLocal"",
                  CASE
                    WHEN fi2.""InitLocal"" > COALESCE(lag(fi2.""EndLocal"") OVER (PARTITION BY fi2.""AreaName"" ORDER BY fi2.""InitLocal"", fi2.""EndLocal""), fi2.""InitLocal"")
                      THEN 1 ELSE 0
                  END AS change
                FROM final_intervals fi2
              ) sub
                ON sub.""AreaName"" = fi.""AreaName""
               AND sub.""InitLocal"" = fi.""InitLocal""
               AND sub.""EndLocal""  = fi.""EndLocal""
            ),
            final_unified_intervals AS (
              SELECT
                ug.""WhenLocal"",
                min(ug.""InitLocal"") AS ""InitLocal"",
                max(ug.""EndLocal"")  AS ""EndLocal"",
                ug.""AreaName""
              FROM unified_groups ug
              GROUP BY ug.""AreaName"", ug.""WhenLocal"", ug.group_id
            ),
            agg_local AS (
              SELECT
                fui.""WhenLocal"",
                fui.""AreaName"",
                sum(EXTRACT(epoch FROM (fui.""EndLocal"" - fui.""InitLocal""))) / 3600::numeric AS ""AreasPercUt""
              FROM final_unified_intervals fui
              GROUP BY fui.""WhenLocal"", fui.""AreaName""
            )
            SELECT
              h.hora_local AS ""When"",               
              a.""AreaName"",
              COALESCE(al.""AreasPercUt"", 0::numeric) AS ""AreasPercUt""
            FROM horas_local h
            CROSS JOIN (
              SELECT DISTINCT ""Name"" AS ""AreaName""
              FROM public.""Areas""
            ) a
            LEFT JOIN agg_local al
              ON al.""WhenLocal"" = h.hora_local
             AND al.""AreaName""  = a.""AreaName""
            ORDER BY 1, 2;
            $function$
            ;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS public.areaview_p(uuid, text, text);");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS public.planningresourcesview_p(uuid, text, text);");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS public.processinterleavingview_p(uuid, text, text);");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS public.resourcesusageview_p(uuid, interval, text);");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS public.workforcevsavailabilityview_p(uuid, interval, text);");

            //Resources Usage View
            migrationBuilder.Sql(@"CREATE OR REPLACE VIEW public.resourcesusageview
            AS WITH plan AS (
                     SELECT DISTINCT ""Plannings"".""Id"",
                        ""Plannings"".""CreationDate""
                       FROM ""Plannings""
                      ORDER BY ""Plannings"".""CreationDate"" DESC
                     LIMIT 1
                    ), itemplanning AS (
                     SELECT ip.""Id"",
                        ip.""InitDate"",
                        ip.""EndDate"",
                        ip.""WorkTime"",
                        ip.""IsStored"",
                        ip.""IsBlocked"",
                        ip.""IsStarted"",
                        ip.""WorkOrderPlanningId"",
                        ip.""WorkerId"",
                        ro.""Name"" AS ""ResourceName""
                       FROM plan p
                         JOIN ""WorkOrdersPlanning"" wo ON p.""Id"" = wo.""PlanningId""
                         JOIN ""ItemsPlanning"" ip ON wo.""Id"" = ip.""WorkOrderPlanningId""
                         JOIN ""Processes"" po ON ip.""ProcessId"" = po.""Id""
                         JOIN ""Areas"" a_1 ON po.""AreaId"" = a_1.""Id""
                         LEFT JOIN ""Workers"" w ON ip.""WorkerId"" = w.""Id""
                         FULL JOIN ""Roles"" ro ON w.""RolId"" = ro.""Id""
                    ), limits AS (
                     SELECT date_trunc('day'::text, now()) AS firsthour,
                        date_trunc('hour'::text, max(itemplanning.""EndDate"")) + '01:00:00'::interval AS lasthour
                       FROM itemplanning
                    ), horas AS (
                     SELECT generate_series(( SELECT limits.firsthour
                               FROM limits), ( SELECT limits.lasthour
                               FROM limits), '01:00:00'::interval) AS hora
                    ), intervals AS (
                     SELECT DISTINCT itemplanning.""InitDate"",
                        itemplanning.""EndDate"",
                        itemplanning.""ResourceName""
                       FROM itemplanning
                    ), cortes AS (
                     SELECT h_1.hora,
                        i.""ResourceName"",
                        GREATEST(i.""InitDate"", h_1.hora) AS ""InitDate"",
                        LEAST(i.""EndDate"", h_1.hora + '01:00:00'::interval) AS ""EndDate""
                       FROM intervals i
                         JOIN horas h_1 ON i.""InitDate"" < (h_1.hora + '01:00:00'::interval) AND i.""EndDate"" >= h_1.hora
                    ), groups AS (
                     SELECT sub.""ResourceName"",
                        sub.""InitDate"",
                        sub.""EndDate"",
                        sum(sub.change) OVER (PARTITION BY sub.""ResourceName"" ORDER BY sub.""InitDate"") AS group_id
                       FROM ( SELECT cortes.hora,
                                cortes.""ResourceName"",
                                cortes.""InitDate"",
                                cortes.""EndDate"",
                                    CASE
                                        WHEN cortes.""InitDate"" > COALESCE(lag(cortes.""EndDate"") OVER (PARTITION BY cortes.""ResourceName"" ORDER BY cortes.""InitDate""), cortes.""InitDate"") THEN 1
                                        ELSE 0
                                    END AS change
                               FROM cortes) sub
                    ), grouped_intervals AS (
                     SELECT groups.""ResourceName"",
                        min(groups.""InitDate"") AS ""InitDate"",
                        max(groups.""EndDate"") AS ""EndDate""
                       FROM groups
                      GROUP BY groups.""ResourceName"", groups.group_id
                    ), final_intervals AS (
                     SELECT sub.""ResourceName"",
                        sub.truncated_date AS hour,
                        GREATEST(sub.""InitDate"", sub.truncated_date) AS ""InitDate"",
                        LEAST(sub.""EndDate"", sub.truncated_date + '01:00:00'::interval) AS ""EndDate""
                       FROM ( SELECT grouped_intervals.""ResourceName"",
                                grouped_intervals.""InitDate"",
                                grouped_intervals.""EndDate"",
                                generate_series(date_trunc('hour'::text, grouped_intervals.""InitDate""), date_trunc('hour'::text, grouped_intervals.""EndDate""), '01:00:00'::interval) AS truncated_date
                               FROM grouped_intervals) sub
                    ), unified_groups AS (
                     SELECT sub.""ResourceName"",
                        sub.hour AS ""When"",
                        sub.""InitDate"",
                        sub.""EndDate"",
                        sum(sub.change) OVER (PARTITION BY sub.""ResourceName"" ORDER BY sub.""InitDate"", sub.""EndDate"") AS group_id
                       FROM ( SELECT final_intervals.""ResourceName"",
                                final_intervals.hour,
                                final_intervals.""InitDate"",
                                final_intervals.""EndDate"",
                                    CASE
                                        WHEN final_intervals.""InitDate"" > COALESCE(lag(final_intervals.""EndDate"") OVER (PARTITION BY final_intervals.""ResourceName"" ORDER BY final_intervals.""InitDate"", final_intervals.""EndDate""), final_intervals.""InitDate"") THEN 1
                                        ELSE 0
                                    END AS change
                               FROM final_intervals) sub
                    ), final_unified_intervals AS (
                     SELECT unified_groups.""When"",
                        min(unified_groups.""InitDate"") AS ""InitDate"",
                        max(unified_groups.""EndDate"") AS ""EndDate"",
                        unified_groups.""ResourceName""
                       FROM unified_groups
                      GROUP BY unified_groups.""ResourceName"", unified_groups.""When"", unified_groups.group_id
                    )
             SELECT h.hora AS ""When"",
                a.""ResourceName"",
                COALESCE(fi.""ResourcesUt"", 0::numeric) AS resourcesut
               FROM horas h
                 CROSS JOIN ( SELECT DISTINCT ""Roles"".""Name"" AS ""ResourceName""
                       FROM ""Roles"") a
                 LEFT JOIN ( SELECT DISTINCT final_unified_intervals.""When"",
                        final_unified_intervals.""ResourceName"",
                        sum(EXTRACT(epoch FROM final_unified_intervals.""EndDate"" - final_unified_intervals.""InitDate"")) / 3600::numeric AS ""ResourcesUt""
                       FROM final_unified_intervals
                      GROUP BY final_unified_intervals.""When"", final_unified_intervals.""ResourceName""
                      ORDER BY final_unified_intervals.""When"", final_unified_intervals.""ResourceName"") fi ON h.hora = fi.""When"" AND a.""ResourceName"" = fi.""ResourceName""
              ORDER BY h.hora, a.""ResourceName"";");

            //Workforce Availability View
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
                        w.""Name"" AS workername,
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
                        intervals_withgroups.""EndDate"" - intervals_withgroups.""InitDate"" AS worktime,
                        intervals_withgroups.prev_init_date,
                        intervals_withgroups.prev_end_date,
                        intervals_withgroups.is_new_group,
                        sum(intervals_withgroups.is_new_group) OVER (PARTITION BY intervals_withgroups.""Name"", intervals_withgroups.hour ORDER BY intervals_withgroups.""InitDate"") AS grupo
                       FROM intervals_withgroups
                    ), intervals_merged AS (
                     SELECT intervals_groups.""Name"",
                        intervals_groups.workername,
                        intervals_groups.hour,
                        min(intervals_groups.""InitDate"") AS ""InitDate"",
                        max(intervals_groups.""EndDate"") AS ""EndDate"",
                        sum(intervals_groups.worktime) AS worktime
                       FROM intervals_groups
                      GROUP BY intervals_groups.""Name"", intervals_groups.workername, intervals_groups.hour, intervals_groups.grupo
                    ), intervals_merged_grouped AS (
                     SELECT intervals_merged.""Name"",
                        intervals_merged.hour,
                        sum(intervals_merged.worktime) AS worktime
                       FROM intervals_merged
                      GROUP BY intervals_merged.""Name"", intervals_merged.hour
                    ), workershifts AS (
                     SELECT r.""Name"",
                        sc.""AvailableWorkerId"",
                        sh.""InitHour"",
                        sh.""EndHour"",
                        sh.""WarehouseId""
                       FROM ""Schedules"" sc
                         JOIN ""Shifts"" sh ON sc.""ShiftId"" = sh.""Id""
                         JOIN ""AvailableWorkers"" a ON sc.""AvailableWorkerId"" = a.""Id""
                         JOIN ""Workers"" w ON a.""WorkerId"" = w.""Id""
                         JOIN ""Roles"" r ON w.""RolId"" = r.""Id""
                    ), workershiftsforlastplanning AS (
                     SELECT w.""Name"",
                        w.""AvailableWorkerId"",
                        w.""InitHour"",
                        w.""EndHour""
                       FROM workershifts w
                         JOIN plan p ON w.""WarehouseId"" = p.""WarehouseId""
                    ), horasworkershifts AS (
                     SELECT generate_series(
                            CASE
                                WHEN date_part('hour'::text, min(itemplanning.""InitDate"")) > w.""InitHour"" THEN floor(w.""InitHour"")::integer
                                ELSE floor(date_part('hour'::text, min(itemplanning.""InitDate"")))::integer
                            END,
                            CASE
                                WHEN date_part('hour'::text, max(itemplanning.""EndDate"")) > w.""EndHour"" THEN ceiling(date_part('hour'::text, max(itemplanning.""EndDate"")))::integer
                                ELSE ceiling(w.""EndHour"")::integer
                            END, 1) AS hour,
                        w.""AvailableWorkerId"",
                        w.""Name""
                       FROM workershiftsforlastplanning w,
                        itemplanning
                      GROUP BY w.""AvailableWorkerId"", w.""Name"", w.""InitHour"", w.""EndHour""
                    ), intervals_grouped AS (
                     SELECT horasworkershifts.hour,
                        horasworkershifts.""Name"",
                        count(*) * 3600 AS availabilitytime
                       FROM horasworkershifts
                      GROUP BY horasworkershifts.""Name"", horasworkershifts.hour
                    )
             SELECT h.""Name"",
                h.""When"",
                COALESCE(wt.worktime * 100::numeric / h.availabilitytime::numeric, 0::numeric) AS workforcevsavailability
               FROM ( SELECT date_trunc('day'::text, now()) + intervals_grouped.hour::double precision * '01:00:00'::interval AS ""When"",
                        intervals_grouped.""Name"",
                        intervals_grouped.availabilitytime
                       FROM intervals_grouped) h
                 LEFT JOIN ( SELECT intervals_merged_grouped.""Name"",
                        intervals_merged_grouped.hour AS ""When"",
                        COALESCE(EXTRACT(epoch FROM intervals_merged_grouped.worktime), 0::numeric) AS worktime
                       FROM intervals_merged_grouped) wt ON h.""When"" = wt.""When"" AND h.""Name"" = wt.""Name""
              ORDER BY h.""Name"", h.""When"";
             ");

            //Process Interleaving View
            migrationBuilder.Sql(@"CREATE OR REPLACE VIEW public.processinterleavingview
            AS WITH plan AS (
                     SELECT DISTINCT ""Plannings"".""Id"",
                        ""Plannings"".""CreationDate""
                       FROM ""Plannings""
                      ORDER BY ""Plannings"".""CreationDate"" DESC
                     LIMIT 1
                    ), itemplanning AS (
                     SELECT ip.""Id"",
                        ip.""ProcessId"",
                        ip.""IsOutbound"",
                        ip.""LimitDate"",
                        ip.""InitDate"",
                        ip.""EndDate"",
                        ip.""WorkTime"",
                        ip.""IsStored"",
                        ip.""IsBlocked"",
                        ip.""IsStarted"",
                        ip.""WorkOrderPlanningId"",
                        ip.""WorkerId"",
                        ip.""EquipmentGroupId""
                       FROM plan p
                         JOIN ""WorkOrdersPlanning"" wo ON p.""Id"" = wo.""PlanningId""
                         JOIN ""ItemsPlanning"" ip ON wo.""Id"" = ip.""WorkOrderPlanningId""
                    ), limits AS (
                     SELECT date_trunc('day'::text, now()) AS firsthour,
                        date_trunc('hour'::text, max(itemplanning.""EndDate"")) + '01:00:00'::interval AS lasthour
                       FROM itemplanning
                    ), horas AS (
                     SELECT generate_series(( SELECT limits.firsthour
                               FROM limits), ( SELECT limits.lasthour
                               FROM limits), '01:00:00'::interval) AS hour
                    ), intervals AS (
                     SELECT DISTINCT itemplanning.""InitDate"",
                        itemplanning.""EndDate"",
                        itemplanning.""IsOutbound""
                       FROM itemplanning
                    ), intervals_c AS (
                     SELECT h_1.hour,
                        i.""IsOutbound"",
                        GREATEST(i.""InitDate"", h_1.hour) AS ""InitDate"",
                        LEAST(i.""EndDate"", h_1.hour + '01:00:00'::interval) AS ""EndDate""
                       FROM intervals i
                         JOIN horas h_1 ON i.""InitDate"" < (h_1.hour + '01:00:00'::interval) AND i.""EndDate"" >= h_1.hour
                    ), intervals_dup AS (
                     SELECT DISTINCT second.hour,
                        second.""IsOutbound"",
                        second.""InitDate"",
                        second.""EndDate""
                       FROM ( SELECT first.hour,
                                first.""IsOutbound"",
                                first.""InitDate"",
                                first.""EndDate"",
                                first.rn1,
                                row_number() OVER (PARTITION BY first.""IsOutbound"", first.hour, first.""EndDate"" ORDER BY first.""InitDate"") AS rn2
                               FROM ( SELECT intervals_c.hour,
                                        intervals_c.""IsOutbound"",
                                        intervals_c.""InitDate"",
                                        intervals_c.""EndDate"",
                                        row_number() OVER (PARTITION BY intervals_c.""IsOutbound"", intervals_c.hour, intervals_c.""InitDate"" ORDER BY intervals_c.""EndDate"" DESC) AS rn1
                                       FROM intervals_c) first
                              WHERE first.rn1 = 1) second
                      WHERE second.rn2 = 1
                      ORDER BY second.""IsOutbound"", second.hour, second.""InitDate"", second.""EndDate""
                    ), intervals_lag AS (
                     SELECT intervals_dup.hour,
                        intervals_dup.""IsOutbound"",
                        intervals_dup.""InitDate"",
                        intervals_dup.""EndDate"",
                        lag(intervals_dup.""InitDate"") OVER (PARTITION BY intervals_dup.hour, intervals_dup.""IsOutbound"" ORDER BY intervals_dup.""InitDate"") AS prev_init_date,
                        max(intervals_dup.""EndDate"") OVER (PARTITION BY intervals_dup.hour, intervals_dup.""IsOutbound"" ORDER BY intervals_dup.""InitDate"" ROWS BETWEEN UNBOUNDED PRECEDING AND 1 PRECEDING) AS prev_end_date
                       FROM intervals_dup
                    ), intervals_withgroups AS (
                     SELECT intervals_lag.hour,
                        intervals_lag.""IsOutbound"",
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
                     SELECT intervals_withgroups.hour,
                        intervals_withgroups.""IsOutbound"",
                        intervals_withgroups.""InitDate"",
                        intervals_withgroups.""EndDate"",
                        intervals_withgroups.prev_init_date,
                        intervals_withgroups.prev_end_date,
                        intervals_withgroups.is_new_group,
                        sum(intervals_withgroups.is_new_group) OVER (PARTITION BY intervals_withgroups.hour, intervals_withgroups.""IsOutbound"" ORDER BY intervals_withgroups.""InitDate"") AS grupo
                       FROM intervals_withgroups
                    ), intervals_merged AS (
                     SELECT intervals_groups.hour,
                        intervals_groups.""IsOutbound"",
                        min(intervals_groups.""InitDate"") AS ""InitDate"",
                        max(intervals_groups.""EndDate"") AS ""EndDate""
                       FROM intervals_groups
                      GROUP BY intervals_groups.hour, intervals_groups.""IsOutbound"", intervals_groups.grupo
                    ), intervals_relag AS (
                     SELECT intervals_merged.hour,
                        intervals_merged.""IsOutbound"",
                        intervals_merged.""InitDate"",
                        intervals_merged.""EndDate"",
                        lag(intervals_merged.""EndDate"") OVER (PARTITION BY intervals_merged.hour, intervals_merged.""IsOutbound"" ORDER BY intervals_merged.""InitDate"") AS prev_end_date,
                        lag(intervals_merged.""InitDate"") OVER (PARTITION BY intervals_merged.hour, intervals_merged.""IsOutbound"" ORDER BY intervals_merged.""EndDate"") AS prev_init_date
                       FROM intervals_merged
                    ), intervals_regrouped AS (
                     SELECT intervals_relag.hour,
                        intervals_relag.""IsOutbound"",
                        intervals_relag.""InitDate"",
                        intervals_relag.""EndDate"",
                        intervals_relag.prev_end_date,
                        intervals_relag.prev_init_date,
                            CASE
                                WHEN intervals_relag.prev_end_date IS NULL THEN 1
                                WHEN intervals_relag.""InitDate"" > intervals_relag.prev_end_date THEN 1
                                ELSE 0
                            END AS is_new_group
                       FROM intervals_relag
                    ), intervals_regrupo AS (
                     SELECT intervals_regrouped.hour,
                        intervals_regrouped.""IsOutbound"",
                        intervals_regrouped.""InitDate"",
                        intervals_regrouped.""EndDate"",
                        intervals_regrouped.prev_end_date,
                        intervals_regrouped.prev_init_date,
                        intervals_regrouped.is_new_group,
                        sum(intervals_regrouped.is_new_group) OVER (PARTITION BY intervals_regrouped.hour, intervals_regrouped.""IsOutbound"" ORDER BY intervals_regrouped.""InitDate"") AS grupo
                       FROM intervals_regrouped
                    ), intervals_final AS (
                     SELECT intervals_regrupo.hour,
                        intervals_regrupo.""IsOutbound"",
                        min(intervals_regrupo.""InitDate"") AS ""InitDate"",
                        max(intervals_regrupo.""EndDate"") AS ""EndDate""
                       FROM intervals_regrupo
                      GROUP BY intervals_regrupo.hour, intervals_regrupo.""IsOutbound"", intervals_regrupo.grupo
                    ), combined AS (
                     SELECT t.hour,
                        GREATEST(t.""InitDate"", f.""InitDate"") AS ""InitDate"",
                        LEAST(t.""EndDate"", f.""EndDate"") AS ""EndDate""
                       FROM intervals_final t
                         JOIN intervals_final f ON t.""IsOutbound"" = true AND f.""IsOutbound"" = false AND t.""InitDate"" < f.""EndDate"" AND t.""EndDate"" > f.""InitDate""
                    )
             SELECT h.hour AS ""When"",
                (COALESCE(b.inboundtime, 0::numeric) - COALESCE(c.interleavingtime, 0::numeric)) / 3600::numeric AS ""InboundSec"",
                (COALESCE(a.outboundtime, 0::numeric) - COALESCE(c.interleavingtime, 0::numeric)) / 3600::numeric AS ""OutboundSec"",
                COALESCE(c.interleavingtime, 0::numeric) / 3600::numeric AS ""InterleavingSec"",
                (3600::numeric - (COALESCE(a.outboundtime, 0::numeric) - COALESCE(c.interleavingtime, 0::numeric) + (COALESCE(b.inboundtime, 0::numeric) - COALESCE(c.interleavingtime, 0::numeric)) + COALESCE(c.interleavingtime, 0::numeric))) / 3600::numeric AS ""NoWorkingSec""
               FROM horas h
                 LEFT JOIN ( SELECT intervals_final.hour,
                        sum(
                            CASE
                                WHEN intervals_final.""IsOutbound"" = true THEN EXTRACT(epoch FROM intervals_final.""EndDate"" - intervals_final.""InitDate"")
                                ELSE 0::numeric
                            END) AS outboundtime
                       FROM intervals_final
                      GROUP BY intervals_final.hour) a ON h.hour = a.hour
                 LEFT JOIN ( SELECT intervals_final.hour,
                        sum(
                            CASE
                                WHEN intervals_final.""IsOutbound"" = false THEN EXTRACT(epoch FROM intervals_final.""EndDate"" - intervals_final.""InitDate"")
                                ELSE 0::numeric
                            END) AS inboundtime
                       FROM intervals_final
                      GROUP BY intervals_final.hour) b ON COALESCE(a.hour, h.hour) = b.hour
                 LEFT JOIN ( SELECT combined.hour,
                        sum(EXTRACT(epoch FROM combined.""EndDate"" - combined.""InitDate"")) AS interleavingtime
                       FROM combined
                      GROUP BY combined.hour) c ON COALESCE(a.hour, b.hour, h.hour) = c.hour
              ORDER BY a.hour;");

            //Planning Resources View
            migrationBuilder.Sql(@"CREATE OR REPLACE VIEW public.planningresourcesview
            AS WITH plan AS (
                     SELECT DISTINCT ""Plannings"".""Id"",
                        ""Plannings"".""CreationDate""
                       FROM ""Plannings""
                      ORDER BY ""Plannings"".""CreationDate"" DESC
                     LIMIT 1
                    ), itemplanning AS (
                     SELECT ip.""Id"",
                        ip.""ProcessId"",
                        ip.""IsOutbound"",
                        ip.""LimitDate"",
                        ip.""InitDate"",
                        ip.""EndDate"",
                        ip.""WorkTime"",
                        ip.""IsStored"",
                        ip.""IsBlocked"",
                        ip.""IsStarted"",
                        ip.""WorkOrderPlanningId"",
                        ip.""WorkerId"",
                        ip.""EquipmentGroupId"",
                        p.""Id""
                       FROM plan p
                         JOIN ""WorkOrdersPlanning"" wo ON p.""Id"" = wo.""PlanningId""
                         JOIN ""ItemsPlanning"" ip ON wo.""Id"" = ip.""WorkOrderPlanningId""
                    ), limits AS (
                     SELECT date_trunc('day'::text, now()) AS firsthour,
                        date_trunc('hour'::text, max(itemplanning.""EndDate"")) + '01:00:00'::interval AS lasthour
                       FROM itemplanning itemplanning(""Id"", ""ProcessId"", ""IsOutbound"", ""LimitDate"", ""InitDate"", ""EndDate"", ""WorkTime"", ""IsStored"", ""IsBlocked"", ""IsStarted"", ""WorkOrderPlanningId"", ""WorkerId"", ""EquipmentGroupId"", ""Id_1"")
                    ), hours AS (
                     SELECT generate_series(( SELECT limits.firsthour
                               FROM limits), ( SELECT limits.lasthour
                               FROM limits), '01:00:00'::interval) AS hora
                    ), worksbyhour AS (
                     SELECT h.hora,
                        i.""InitDate"",
                        i.""EndDate"",
                        i.""ProcessId"",
                        i.""EquipmentGroupId"",
                        i.""WorkerId""
                       FROM hours h
                         LEFT JOIN itemplanning i(""Id"", ""ProcessId"", ""IsOutbound"", ""LimitDate"", ""InitDate"", ""EndDate"", ""WorkTime"", ""IsStored"", ""IsBlocked"", ""IsStarted"", ""WorkOrderPlanningId"", ""WorkerId"", ""EquipmentGroupId"", ""Id_1"") ON i.""InitDate"" < (h.hora + '01:00:00'::interval) AND i.""EndDate"" >= h.hora
                    ), warehousebyhour AS (
                     SELECT h.hora,
                        i.""InitDate"",
                        i.""EndDate"",
                        i.""ProcessId"",
                        i.""EquipmentGroupId"",
                        i.""WorkerId""
                       FROM hours h
                         LEFT JOIN ""WarehouseProcessPlanning"" i ON i.""InitDate"" < (h.hora + '01:00:00'::interval) AND i.""EndDate"" >= h.hora
                    ), usedbyhour AS (
                     SELECT warehousebyhour.hora,
                        warehousebyhour.""InitDate"",
                        warehousebyhour.""EndDate"",
                        warehousebyhour.""ProcessId"",
                        warehousebyhour.""EquipmentGroupId"",
                        warehousebyhour.""WorkerId""
                       FROM warehousebyhour
                    UNION ALL
                     SELECT worksbyhour.hora,
                        worksbyhour.""InitDate"",
                        worksbyhour.""EndDate"",
                        worksbyhour.""ProcessId"",
                        worksbyhour.""EquipmentGroupId"",
                        worksbyhour.""WorkerId""
                       FROM worksbyhour
                    ), workersbyrol AS (
                     SELECT w.""RolId"",
                        count(DISTINCT aw.""WorkerId"") AS ""AvailableWorkers""
                       FROM ""Workers"" w
                         JOIN ""AvailableWorkers"" aw ON w.""Id"" = aw.""WorkerId""
                      GROUP BY w.""RolId""
                    ), allinformation AS (
                     SELECT ua.""When"",
                        ua.""InitDate"",
                        ua.""EndDate"",
                        ua.""ProcessId"",
                        ua.""ProcessName"",
                        ua.""ResourceType"",
                        ua.""ResourceId"",
                        ua.""ResourceName"",
                        ua.""AvailableResources""
                       FROM ( SELECT u.hora AS ""When"",
                                u.""InitDate"",
                                u.""EndDate"",
                                u.""ProcessId"",
                                pc.""Name"" AS ""ProcessName"",
                                'Operator'::text AS ""ResourceType"",
                                u.""WorkerId"" AS ""ResourceId"",
                                ro.""Name"" AS ""ResourceName"",
                                wr.""AvailableWorkers"" AS ""AvailableResources""
                               FROM usedbyhour u
                                 LEFT JOIN ""Workers"" w ON u.""WorkerId"" = w.""Id""
                                 FULL JOIN ""Roles"" ro ON w.""RolId"" = ro.""Id""
                                 FULL JOIN ""Processes"" pc ON u.""ProcessId"" = pc.""Id""
                                 LEFT JOIN ""EquipmentGroups"" eg ON pc.""AreaId"" = eg.""AreaId"" AND eg.""Id"" = u.""EquipmentGroupId""
                                 LEFT JOIN workersbyrol wr ON w.""RolId"" = wr.""RolId""
                            UNION ALL
                             SELECT u.hora AS ""When"",
                                u.""InitDate"",
                                u.""EndDate"",
                                u.""ProcessId"",
                                pc.""Name"" AS ""ProcessName"",
                                'Equipment'::text AS ""ResourceType"",
                                eg.""TypeEquipmentId"" AS ""ResourceId"",
                                eg.""Name"" AS ""ResourceName"",
                                eg.""Equipments"" AS ""AvailableResources""
                               FROM usedbyhour u
                                 FULL JOIN ""Processes"" pc ON u.""ProcessId"" = pc.""Id""
                                 LEFT JOIN ""EquipmentGroups"" eg ON pc.""AreaId"" = eg.""AreaId"" AND eg.""Id"" = u.""EquipmentGroupId"") ua
                    ), resourcesandprocess AS (
                     SELECT DISTINCT h.""When"",
                        a_1.""Name"" AS ""ProcessName"",
                        b.""Name"" AS ""ResourceName""
                       FROM allinformation h
                         CROSS JOIN ""Processes"" a_1
                         CROSS JOIN ""Roles"" b
                    ), availableresourcesnulls AS (
                     SELECT DISTINCT ai.""ProcessName"",
                        ai.""ResourceName"",
                        COALESCE(max(ai.""AvailableResources""), 0::bigint) AS ""AvailableResources""
                       FROM allinformation ai
                      GROUP BY ai.""ProcessName"", ai.""ResourceName""
                    )
             SELECT r.""When"",
                a.""InitDate"",
                a.""EndDate"",
                a.""ProcessId"",
                r.""ProcessName"",
                a.""ResourceType"",
                a.""ResourceId"",
                r.""ResourceName"",
                COALESCE(a.""AvailableResources"", an.""AvailableResources"", 0::bigint) AS ""AvailableResources""
               FROM resourcesandprocess r
                 LEFT JOIN allinformation a ON r.""When"" = a.""When"" AND r.""ProcessName"" = a.""ProcessName"" AND r.""ResourceName"" = a.""ResourceName""
                 LEFT JOIN availableresourcesnulls an ON r.""ProcessName"" = an.""ProcessName"" AND r.""ResourceName"" = an.""ResourceName""
              WHERE r.""When"" IS NOT NULL
              ORDER BY r.""When"", r.""ProcessName"", r.""ResourceName"";");

            //Area View
            migrationBuilder.Sql(@"CREATE OR REPLACE VIEW public.areaview
            AS WITH plan AS (
                     SELECT DISTINCT ""Plannings"".""Id"",
                        ""Plannings"".""CreationDate""
                       FROM ""Plannings""
                      ORDER BY ""Plannings"".""CreationDate"" DESC
                     LIMIT 1
                    ), itemplanning AS (
                     SELECT ip.""Id"",
                        a_1.""Name"" AS ""AreaName"",
                        ip.""InitDate"",
                        ip.""EndDate"",
                        ip.""WorkTime"",
                        ip.""IsStored"",
                        ip.""IsBlocked"",
                        ip.""IsStarted"",
                        ip.""WorkOrderPlanningId"",
                        ip.""WorkerId"",
                        ip.""EquipmentGroupId""
                       FROM plan p
                         JOIN ""WorkOrdersPlanning"" wo ON p.""Id"" = wo.""PlanningId""
                         JOIN ""ItemsPlanning"" ip ON wo.""Id"" = ip.""WorkOrderPlanningId""
                         JOIN ""Processes"" po ON ip.""ProcessId"" = po.""Id""
                         JOIN ""Areas"" a_1 ON po.""AreaId"" = a_1.""Id""
                    ), limits AS (
                     SELECT date_trunc('day'::text, now()) AS firsthour,
                        date_trunc('hour'::text, max(itemplanning.""EndDate"")) + '01:00:00'::interval AS lasthour
                       FROM itemplanning
                    ), horas AS (
                     SELECT generate_series(( SELECT limits.firsthour
                               FROM limits), ( SELECT limits.lasthour
                               FROM limits), '01:00:00'::interval) AS hora
                    ), intervals AS (
                     SELECT DISTINCT itemplanning.""InitDate"",
                        itemplanning.""EndDate"",
                        itemplanning.""AreaName""
                       FROM itemplanning
                    ), cortes AS (
                     SELECT h_1.hora,
                        i.""AreaName"",
                        GREATEST(i.""InitDate"", h_1.hora) AS ""InitDate"",
                        LEAST(i.""EndDate"", h_1.hora + '01:00:00'::interval) AS ""EndDate""
                       FROM intervals i
                         JOIN horas h_1 ON i.""InitDate"" < (h_1.hora + '01:00:00'::interval) AND i.""EndDate"" >= h_1.hora
                    ), groups AS (
                     SELECT sub.""AreaName"",
                        sub.""InitDate"",
                        sub.""EndDate"",
                        sum(sub.change) OVER (PARTITION BY sub.""AreaName"" ORDER BY sub.""InitDate"") AS group_id
                       FROM ( SELECT cortes.hora,
                                cortes.""AreaName"",
                                cortes.""InitDate"",
                                cortes.""EndDate"",
                                    CASE
                                        WHEN cortes.""InitDate"" > COALESCE(lag(cortes.""EndDate"") OVER (PARTITION BY cortes.""AreaName"" ORDER BY cortes.""InitDate""), cortes.""InitDate"") THEN 1
                                        ELSE 0
                                    END AS change
                               FROM cortes) sub
                    ), grouped_intervals AS (
                     SELECT groups.""AreaName"",
                        min(groups.""InitDate"") AS ""InitDate"",
                        max(groups.""EndDate"") AS ""EndDate""
                       FROM groups
                      GROUP BY groups.""AreaName"", groups.group_id
                    ), final_intervals AS (
                     SELECT sub.""AreaName"",
                        sub.truncated_date AS hour,
                        GREATEST(sub.""InitDate"", sub.truncated_date) AS ""InitDate"",
                        LEAST(sub.""EndDate"", sub.truncated_date + '01:00:00'::interval) AS ""EndDate""
                       FROM ( SELECT grouped_intervals.""AreaName"",
                                grouped_intervals.""InitDate"",
                                grouped_intervals.""EndDate"",
                                generate_series(date_trunc('hour'::text, grouped_intervals.""InitDate""), date_trunc('hour'::text, grouped_intervals.""EndDate""), '01:00:00'::interval) AS truncated_date
                               FROM grouped_intervals) sub
                    ), unified_groups AS (
                     SELECT sub.""AreaName"",
                        sub.hour AS ""When"",
                        sub.""InitDate"",
                        sub.""EndDate"",
                        sum(sub.change) OVER (PARTITION BY sub.""AreaName"" ORDER BY sub.""InitDate"", sub.""EndDate"") AS group_id
                       FROM ( SELECT final_intervals.""AreaName"",
                                final_intervals.hour,
                                final_intervals.""InitDate"",
                                final_intervals.""EndDate"",
                                    CASE
                                        WHEN final_intervals.""InitDate"" > COALESCE(lag(final_intervals.""EndDate"") OVER (PARTITION BY final_intervals.""AreaName"" ORDER BY final_intervals.""InitDate"", final_intervals.""EndDate""), final_intervals.""InitDate"") THEN 1
                                        ELSE 0
                                    END AS change
                               FROM final_intervals) sub
                    ), final_unified_intervals AS (
                     SELECT unified_groups.""When"",
                        min(unified_groups.""InitDate"") AS ""InitDate"",
                        max(unified_groups.""EndDate"") AS ""EndDate"",
                        unified_groups.""AreaName""
                       FROM unified_groups
                      GROUP BY unified_groups.""AreaName"", unified_groups.""When"", unified_groups.group_id
                    )
             SELECT h.hora AS ""When"",
                a.""AreaName"",
                COALESCE(fi.""AreasPercUt"", 0::numeric) AS areaspercut
               FROM horas h
                 CROSS JOIN ( SELECT DISTINCT ""Areas"".""Name"" AS ""AreaName""
                       FROM ""Areas"") a
                 LEFT JOIN ( SELECT DISTINCT final_unified_intervals.""When"",
                        final_unified_intervals.""AreaName"",
                        sum(EXTRACT(epoch FROM final_unified_intervals.""EndDate"" - final_unified_intervals.""InitDate"")) / 3600::numeric AS ""AreasPercUt""
                       FROM final_unified_intervals
                      GROUP BY final_unified_intervals.""When"", final_unified_intervals.""AreaName""
                      ORDER BY final_unified_intervals.""When"", final_unified_intervals.""AreaName"") fi ON h.hora = fi.""When"" AND a.""AreaName"" = fi.""AreaName""
              ORDER BY h.hora, a.""AreaName"";");


        }
    }
}
