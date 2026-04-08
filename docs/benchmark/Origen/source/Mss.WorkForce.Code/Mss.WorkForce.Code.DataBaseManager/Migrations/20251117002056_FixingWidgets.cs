using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mss.WorkForce.Code.DataBaseManager.Migrations
{
    /// <inheritdoc />
    public partial class FixingWidgets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS public.areaview_p(uuid,text, text);");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS public.resourcesusageview_p(uuid,interval, text);");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS public.planningresourcesview_p(uuid, text, text);");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS public.workforcevsavailabilityview_p(uuid, interval, text);");

            //Área Function
            migrationBuilder.Sql(@"
            CREATE OR REPLACE FUNCTION public.areaview_p(_planning_id uuid,_warehouse_id uuid,  _utc_offset text, _hour_format text)
             RETURNS TABLE(""When"" timestamp without time zone, ""AreaName"" text, ""AreasPercUt"" numeric)
             LANGUAGE sql
             STABLE
            AS $function$
            WITH plan AS (
                SELECT p.""Id"", p.""CreationDate"", p.""WarehouseId""
                FROM public.""Plannings"" p
                WHERE p.""Id"" = _planning_id
            ),
            itemplanning AS (
                SELECT
                ip.""Id"",
                a.""Name""                              AS ""AreaName"",
                ip.""InitDate""::timestamptz            AS ""InitDateUTC"",
                ip.""EndDate""::timestamptz             AS ""EndDateUTC""
                FROM plan p
                JOIN public.""WorkOrdersPlanning"" wo ON p.""Id"" = wo.""PlanningId""
                JOIN public.""ItemsPlanning""      ip ON wo.""Id"" = ip.""WorkOrderPlanningId""
                JOIN public.""Processes""          po ON ip.""ProcessId"" = po.""Id""
                JOIN public.""Areas""               a ON po.""AreaId"" = a.""Id""
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
                date_trunc('day',  min(""InitLocal""))                         AS firsthour_local,
                date_trunc('hour', max(""EndLocal"")) + interval '1 hour'      AS lasthour_local
                FROM ip_local
            ),
            horas_local AS (
                -- Serie hasta la ÚLTIMA hora EXCLUIDA para evitar bucket vacío (0 s)
                SELECT generate_series(
                        (SELECT firsthour_local FROM limits),
                        (SELECT lasthour_local  FROM limits) - interval '1 hour',
                        interval '1 hour'
                        ) AS hora_local
            ),
            intervals AS (
                -- Deduplicación defensiva de tramos
                SELECT DISTINCT
                l.""AreaName"",
                l.""InitLocal"",
                l.""EndLocal""
                FROM ip_local l
            ),
            cortes AS (
                -- Cortes por hora
                SELECT
                h.hora_local,
                i.""AreaName"",
                GREATEST(i.""InitLocal"", h.hora_local)                  AS ""InitLocal"",
                LEAST(i.""EndLocal"",  h.hora_local + interval '1 hour') AS ""EndLocal""
                FROM intervals i
                JOIN horas_local h
                ON i.""InitLocal"" < (h.hora_local + interval '1 hour')
                AND i.""EndLocal""  >= h.hora_local
            ),
            prev_date AS (
                -- Ventanas PARTICIONADAS por (Área, HORA)
                SELECT
                c.""AreaName"",
                c.hora_local,
                c.""InitLocal"",
                c.""EndLocal"",
                lag(c.""InitLocal"") OVER (
                    PARTITION BY c.""AreaName"", c.hora_local
                    ORDER BY c.""InitLocal""
                ) AS prev_init_local,
                max(c.""EndLocal"") OVER (
                    PARTITION BY c.""AreaName"", c.hora_local
                    ORDER BY c.""InitLocal""
                    ROWS BETWEEN UNBOUNDED PRECEDING AND 1 PRECEDING
                ) AS prev_end_local
                FROM cortes c
            ),
            groupid_assigned AS (
                SELECT
                p.hora_local,
                p.""AreaName"",
                p.""InitLocal"",
                p.""EndLocal"",
                CASE
                    WHEN p.prev_end_local IS NULL THEN 1
                    WHEN p.""InitLocal"" > p.prev_end_local THEN 1
                    ELSE 0
                END AS change
                FROM prev_date p
            ),
            groups AS (
                -- ID de grupo acumulado por (Área, HORA)
                SELECT
                g.hora_local,
                g.""AreaName"",
                g.""InitLocal"",
                g.""EndLocal"",
                SUM(g.change) OVER (
                    PARTITION BY g.""AreaName"", g.hora_local
                    ORDER BY g.""InitLocal""
                ) AS group_id
                FROM groupid_assigned g
            ),
            grouped_intervals AS (
                -- Tramos ya unificados dentro de cada HORA/Área
                SELECT
                gr.""AreaName"",
                gr.hora_local       AS ""WhenLocal"",
                MIN(gr.""InitLocal"") AS ""InitLocal"",
                MAX(gr.""EndLocal"")  AS ""EndLocal""
                FROM groups gr
                GROUP BY gr.""AreaName"", gr.hora_local, gr.group_id
            ),
            agg_local AS (
                -- Suma de segundos, capada a 3600 s (100%) por hora/área
                SELECT
                gi.""WhenLocal"",
                gi.""AreaName"",
                LEAST(
                    SUM(EXTRACT(EPOCH FROM (gi.""EndLocal"" - gi.""InitLocal""))),
                    3600
                ) / 3600::numeric AS ""AreasPercUt""
                FROM grouped_intervals gi
                GROUP BY gi.""WhenLocal"", gi.""AreaName""
            )
            SELECT
                h.hora_local                                AS ""When"",
                a.""AreaName"",
                COALESCE(al.""AreasPercUt"", 0::numeric)      AS ""AreasPercUt""
            FROM horas_local h
            CROSS JOIN (
                SELECT DISTINCT ar.""Name"" AS ""AreaName"" FROM public.""Areas"" ar JOIN public.""Layouts"" l ON ar.""LayoutId""=l.""Id"" WHERE l.""WarehouseId""=_warehouse_id) a
            LEFT JOIN agg_local al
                ON al.""WhenLocal"" = h.hora_local
                AND al.""AreaName""  = a.""AreaName""
            ORDER BY 1, 2;
            $function$
            ;");

            //Resource Usage Function 
            migrationBuilder.Sql(@"
            CREATE OR REPLACE FUNCTION public.resourcesusageview_p(_planning_id uuid, _warehouse_id uuid, _utc_offset interval DEFAULT '00:00:00'::interval, _hour_fmt text DEFAULT 'HH24:MI'::text)
             RETURNS TABLE(""Name"" text, ""When"" timestamp without time zone, workforcevsavailability numeric, worktime numeric, availabilitytime numeric, when_utc_txt text)
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
                    ip.""InitDate""::timestamptz               AS ""InitDate"",
                    ip.""EndDate"" ::timestamptz               AS ""EndDate"",
                    w.""Id"" AS ""WorkerId"",
                    w.""Name"" AS workername,
                    r.""Name""
              FROM plan p
              JOIN public.""WorkOrdersPlanning"" wo ON p.""Id"" = wo.""PlanningId""
              JOIN public.""ItemsPlanning"" ip      ON wo.""Id"" = ip.""WorkOrderPlanningId""
              JOIN public.""Workers"" w                     ON ip.""WorkerId"" = w.""Id""
              JOIN public.""Roles""   r                     ON w.""RolId"" = r.""Id""
            ),

            workershiftsforlastplanning AS (
              SELECT r.""Name"",
                    sc.""AvailableWorkerId"",
                    w.""Id"" AS ""WorkerId"",
                    sh.""InitHour"",
                    sh.""EndHour"",
                    sh.""WarehouseId""
              FROM public.""Schedules"" sc 
              JOIN public.""Shifts"" sh ON sc.""ShiftId"" = sh.""Id""
              JOIN plan p ON sh.""WarehouseId"" = p.""WarehouseId""
              JOIN public.""AvailableWorkers"" a ON sc.""AvailableWorkerId"" = a.""Id""
              JOIN public.""Workers"" w ON a.""WorkerId"" = w.""Id""
              JOIN public.""Roles"" r ON w.""RolId"" = r.""Id""
            ),

            workershiftsforlastplanning_extra AS ( --Si se trabaja fuera de turno se quiere contar como available time, se asume que solo pasa esto seguido del turno por lo que se amplia con lo que haya en ip
                SELECT
                  w.""Name"",
                  w.""AvailableWorkerId"",
                  w.""WorkerId"",
                  CASE WHEN MIN(ip.""InitDate"") IS NOT NULL THEN
                  (EXTRACT(EPOCH FROM(LEAST(MIN(ip.""InitDate""),date_trunc('day', MIN(ip.""InitDate"")) + (w.""InitHour"" * interval '1 hour'))-date_trunc('day', MIN(ip.""InitDate""))))/3600)::numeric
                  ELSE 
                  w.""InitHour""::numeric
                  END AS ""InitHour"", 

                  CASE WHEN MAX(ip.""EndDate"") IS NOT NULL THEN
                  (EXTRACT(EPOCH FROM (GREATEST(MAX(ip.""EndDate""),date_trunc('day', MAX(ip.""EndDate"")) + (w.""EndHour"" * interval '1 hour'))-date_trunc('day', MAX(ip.""EndDate""))))/3600)::numeric
                  ELSE
                  W.""EndHour""::numeric
                  END AS ""EndHour"",

                  w.""WarehouseId""
              FROM workershiftsforlastplanning w
              LEFT JOIN itemplanning ip
              ON w.""WorkerId""=ip.""WorkerId""
                  GROUP BY 
                    w.""Name"",
                    w.""AvailableWorkerId"",
                    w.""WorkerId"",
                    w.""InitHour"",
                    w.""EndHour"",
                    w.""WarehouseId""
            ),

            horas_roles AS ( --Esto hace falta porque para que el gráfico vaya bien todas los roles deben aparecer en todas las horas. Se hace aquí para hacer más abajo un LEFt JOIN con esto.
              SELECT 
               generate_series(
                floor(MIN(w.""InitHour""))::int,
                ceiling(MAX(w.""EndHour""))::int,
                1) AS hour
              FROM workershiftsforlastplanning_extra w
            ),

            breaks AS(
              SELECT r.""Name"",
                    sc.""AvailableWorkerId"",
                    w.""Id"" AS ""WorkerId"",
                    w.""Name"" AS workername,
                    br.""InitBreak"",
                    br.""EndBreak""
            FROM public.""Schedules"" sc      
            JOIN public.""AvailableWorkers"" a ON sc.""AvailableWorkerId"" = a.""Id""
            JOIN public.""Workers"" w ON a.""WorkerId"" = w.""Id""
            JOIN public.""Roles"" r ON w.""RolId"" = r.""Id""
            JOIN plan p ON r.""WarehouseId""=p.""WarehouseId""
            JOIN public.""BreakProfiles"" bp ON sc.""BreakProfileId""=bp.""Id""
            JOIN public.""Breaks"" br ON bp.""Id""=br.""BreakProfileId""
            ),

            horas_breaks AS(
                SELECT generate_series(
                        floor(min(br.""InitBreak""))::int,
                        ceiling(max(br.""EndBreak""))::int -1 , 
                        1
                      ) AS hour_break
                FROM breaks br
            ),

            intervals_c_breaks AS (
              SELECT br.""Name"",
                     br.""AvailableWorkerId"",
                      br.workername,
                      hb.hour_break,
                      GREATEST(br.""InitBreak"", hb.hour_break)    AS ""InitBreak"",
                      LEAST(br.""EndBreak"",   hb.hour_break + 1)  AS ""EndBreak""
              FROM breaks br
              JOIN horas_breaks hb
                ON br.""InitBreak"" < (hb.hour_break + 1)
                AND br.""EndBreak""  >= hb.hour_break
            ),

            breaks_lag AS (
                SELECT
                    icb.""Name"",
                    icb.""AvailableWorkerId"",
                    icb.workername,
                    icb.hour_break,
                    icb.""InitBreak"",
                    icb.""EndBreak"",
                    LAG(icb.""InitBreak"") OVER (PARTITION BY icb.""Name"", icb.""AvailableWorkerId"", icb.hour_break ORDER BY icb.""InitBreak"") AS prev_init,
                    MAX(icb.""EndBreak"") OVER (PARTITION BY icb.""Name"", icb.""AvailableWorkerId"", icb.hour_break ORDER BY icb.""InitBreak"" ROWS BETWEEN UNBOUNDED PRECEDING AND 1 PRECEDING) AS prev_end 
                FROM intervals_c_breaks icb
            ),

            breaks_grouped AS (
                SELECT *,
                    CASE 
                        WHEN prev_end IS NULL THEN 1
                        WHEN ""InitBreak"" > prev_end THEN 1   --No solapan - nuevo grupo
                        ELSE 0                                -- Solapan
                    END AS is_new_group
                FROM breaks_lag
            ),

            breaks_group_id AS (
                SELECT *,
                    SUM(is_new_group) OVER (PARTITION BY ""Name"", ""AvailableWorkerId"", hour_break ORDER BY ""InitBreak"") AS group_id
                FROM breaks_grouped
            ),

            breaks_merged AS ( --breaks en float
                SELECT 
                    ""Name"",
                    ""AvailableWorkerId"",
                    workername,
                    hour_break,
                    MIN(""InitBreak"") AS ""InitBreak"",
                    MAX(""EndBreak"")  AS ""EndBreak""
                FROM breaks_group_id
                GROUP BY 
                    ""Name"", ""AvailableWorkerId"", workername,hour_break, group_id
            ),

            break_time AS( --Los shifts y los breaks deben ser time para poder compararlos con fechas de distinto dia
                SELECT 
                ib.""Name"",
                ib.""hour_break"",
                ib.workername,
                (date_trunc('day', now()) + (ib.""InitBreak"" * interval '1 hour'))::time AS ""InitBreak"",    
                (date_trunc('day', now()) + (ib.""EndBreak"" * interval '1 hour'))::time AS ""EndBreak""
                FROM breaks_merged ib
            ),

            horasworkershifts AS (  --Por cada turno genera un grid con las horas (gs) y va comprobando para cada hora que parte del turno cae dentro, restando el break que se solape)
            SELECT
                w.""AvailableWorkerId"",
                w.""Name"",
                gs.hour AS hour_int,
                GREATEST(0,
                CASE
                  WHEN w.""InitHour"" >= gs.hour AND w.""EndHour"" <= gs.hour + 1 THEN (w.""EndHour"" - w.""InitHour"") -- Caso: turno empieza y termina dentro de la misma hora

                  WHEN gs.hour >= w.""InitHour"" AND gs.hour + 1 <= w.""EndHour"" THEN 1                   -- Si la hora está completamente dentro del turno

                  WHEN w.""InitHour"" >= gs.hour AND w.""InitHour"" < gs.hour + 1 THEN (gs.hour + 1 - w.""InitHour"")       -- Si el turno empieza dentro de esta hora

                  WHEN w.""EndHour"" > gs.hour AND w.""EndHour"" < gs.hour + 1 THEN (w.""EndHour"" - gs.hour)                 -- Si el turno termina dentro de esta hora

                  ELSE 0
                END 
                -SUM(CASE WHEN breaks.""InitBreak"" IS NOT NULL AND breaks.""EndBreak"" IS NOT NULL THEN-- Restar las partes de los breaks que coincidan con el turno
                    GREATEST(0,LEAST(w.""EndHour"", breaks.""EndBreak"") - GREATEST(w.""InitHour"", breaks.""InitBreak""))
                    ELSE 0 
                END)) AS full_hour
                FROM workershiftsforlastplanning_extra w
                CROSS JOIN horas_roles  AS gs
                LEFT JOIN breaks_merged breaks
                ON w.""AvailableWorkerId"" = breaks.""AvailableWorkerId"" 
                AND gs.hour = breaks.hour_break
                GROUP BY w.""AvailableWorkerId"",w.""Name"",gs.hour,w.""InitHour"",w.""EndHour""
            ),

            available_hours AS (
              SELECT
                worker_hour.""Name"",
                worker_hour.hour_int,
                SUM(worker_hour.full_hour) * 3600 AS availabilitytime --Se suman las horas availables de los workers, si no es entera se sumara la parte proporcional
              FROM (
                SELECT
                  hw.""AvailableWorkerId"",
                  hw.""Name"",
                  hw.hour_int,
                  LEAST(SUM(hw.full_hour),1) AS full_hour     --Esto asegura que para cada trabajador y por hora como máximo se coge una hora como available (por si se superpsuieran trabajos)
                FROM horasworkershifts hw
                GROUP BY hw.""AvailableWorkerId"", hw.""Name"", hw.hour_int
              ) AS worker_hour
              GROUP BY worker_hour.""Name"", worker_hour.hour_int
            ),


            limits AS (
              SELECT 
                date_trunc('day', min(itemplanning.""InitDate"")) - interval '1 hour' AS firsthour,
                date_trunc('hour', max(itemplanning.""EndDate"")) + interval '1 hour' AS lasthour
              FROM itemplanning
            ),
            horas AS (
              SELECT generate_series(
                        (SELECT firsthour FROM limits),
                        (SELECT lasthour  FROM limits), 
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
                      lag(ic.""InitDate"") OVER (PARTITION BY ic.""Name"",ic.workername, ic.hour ORDER BY ic.""InitDate"") AS prev_init_date,
                      max(ic.""EndDate"")  OVER (PARTITION BY ic.""Name"",ic.workername, ic.hour ORDER BY ic.""InitDate"" ROWS BETWEEN UNBOUNDED PRECEDING AND 1 PRECEDING) AS prev_end_date
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
                      iwg.prev_init_date,
                      iwg.prev_end_date,
                      iwg.is_new_group,
                      sum(iwg.is_new_group) OVER (PARTITION BY iwg.""Name"", iwg.hour, workername ORDER BY iwg.""InitDate"") AS grupo
              FROM intervals_withgroups iwg
            ),


            intervals_merged AS (
              SELECT ig.""Name"",
                      ig.workername,
                      ig.hour,
                      date_part('hour', ig.hour)::int AS hour_int,
                      min(ig.""InitDate"") AS ""InitDate"",
                      max(ig.""EndDate"")  AS ""EndDate""
              FROM intervals_groups ig
              GROUP BY ig.""Name"", ig.workername, ig.hour, ig.grupo
            ),

            intervals_merged_breaks AS (
              SELECT ig.""Name"",
                      ig.workername,
                      ig.hour,
                     (ig.""EndDate"" - ig.""InitDate"") AS worktime,
                     SUM(CASE WHEN br.""InitBreak"" IS NOT NULL AND br.""EndBreak"" IS NOT NULL THEN
                      GREATEST(INTERVAL '0 seconds',LEAST(ig.""EndDate""::time, br.""EndBreak""::time) - GREATEST(ig.""InitDate""::time, br.""InitBreak""::time))
                     ELSE INTERVAL '0 seconds'
                     END) AS breaktime
              FROM intervals_merged ig
              LEFT JOIN break_time br
              ON ig.workername=br.workername
              AND ig.""hour_int""=br.""hour_break""
              AND ig.""Name""=br.""Name"" -- Esto es supuestamente redundante
              GROUP BY ig.""Name"", ig.workername, ig.hour,ig.""InitDate"",ig.""EndDate""
            ),

            worktime_utc AS (
              SELECT 
                im.""Name"",
                im.hour AS when_utc,
                SUM(EXTRACT(epoch FROM (im.worktime -im.breaktime)))::numeric AS worktime 
              FROM intervals_merged_breaks im
              GROUP BY im.""Name"", im.hour
              ),


            limits_roles AS (
              SELECT
              h.hour AS when_utc,
              date_part('hour', h.hour)::int AS hour_int,
              r.""Name""
              FROM horas h
              CROSS JOIN (SELECT ro.""Name"" FROM public.""Roles"" ro WHERE ro.""WarehouseId""=_warehouse_id) r
            ),

            base AS (
              SELECT lr.""Name"",
                    lr.when_utc,
                    COALESCE(wt.worktime, 0::numeric) AS worktime,
                    COALESCE(s.availabilitytime, 0::numeric) AS availabilitytime
                    FROM limits_roles lr
                    LEFT JOIN worktime_utc wt
                    ON lr.""Name""=wt.""Name""
                    AND lr.when_utc=wt.when_utc
                    LEFT JOIN available_hours s
                      ON lr.""Name"" = s.""Name""
                      AND lr.hour_int = s.hour_int
            )
            SELECT
              b.""Name"",
              ((b.when_utc AT TIME ZONE 'UTC') + _utc_offset) AS ""When"",
              CASE                                                                            --Se disitnguen casos para cuando se trabaja fuera de turno
              WHEN b.availabilitytime=0 AND b.worktime>0 THEN 1
              --WHEN b.availabilitytime=0 AND b.worktime=0 THEN 0
              WHEN b.availabilitytime>0 THEN LEAST(b.worktime::numeric / b.availabilitytime::numeric,1) 
              ELSE 0
              END AS workforcevsavailability,
              b.worktime,
              b.availabilitytime,
              to_char(date_trunc('hour', (b.when_utc AT TIME ZONE 'UTC') + _utc_offset), _hour_fmt) AS when_utc_txt
            FROM base b
            ORDER BY b.""Name"", ""When"";
            $function$;
            ");

            //Planning Function
            migrationBuilder.Sql(@"
            CREATE OR REPLACE FUNCTION public.planningresourcesview_p(_planning_id uuid, _warehouse_id uuid, _utc_offset text, _hour_format text)
             RETURNS TABLE(""When"" timestamp without time zone, ""InitDate"" timestamp without time zone, ""EndDate"" timestamp without time zone, ""ProcessId"" uuid, ""ProcessName"" text, ""ResourceType"" text, ""ResourceId"" uuid, ""ResourceName"" text, ""AvailableResources"" integer)
             LANGUAGE sql
             STABLE
            AS $function$
            WITH plan AS (
                SELECT p.""Id"",p.""WarehouseId""
                FROM public.""Plannings"" p
                WHERE p.""Id"" = _planning_id
            ),

            itemplanning_utc AS (
                SELECT
                ip.""Id"",
                ip.""ProcessId"",
                ip.""IsOutbound"",
                ip.""LimitDate""::timestamptz              AS ""LimitDate"",
                ip.""InitDate""::timestamptz               AS ""InitDate"",
                ip.""EndDate"" ::timestamptz               AS ""EndDate"",
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


            wpp_utc AS (
                SELECT
                w.""ProcessId"",
                w.""EquipmentGroupId"",
                w.""WorkerId"",
                w.""InitDate""::timestamptz AS ""InitDate"",
                w.""EndDate"" ::timestamptz AS ""EndDate""
                FROM public.""WarehouseProcessPlanning"" w WHERE w.""PlanningId""=_planning_id
            ),


            limits AS (
                SELECT
                date_trunc('day', min(x.""InitDate""))                     AS firsthour,
                date_trunc('hour', max(x.""EndDate"")) + interval '1 hour' AS lasthour
                FROM (
                SELECT ""InitDate"",""EndDate"" FROM itemplanning_utc
                UNION ALL
                SELECT ""InitDate"",""EndDate"" FROM wpp_utc
                ) x
            ),
            hours AS (
                SELECT generate_series(
                        (SELECT firsthour FROM limits),
                        (SELECT lasthour  FROM limits),
                        interval '1 hour'
                        ) AS hora
            ),

            breaks AS(
              SELECT r.""Name"",
                    sc.""AvailableWorkerId"",
                    w.""Name"" AS workername,
                    w.""Id"" AS workerid,
                    br.""InitBreak"",
                    br.""EndBreak""
            FROM public.""Schedules"" sc      
            JOIN public.""AvailableWorkers"" a ON sc.""AvailableWorkerId"" = a.""Id""
            JOIN public.""Workers"" w ON a.""WorkerId"" = w.""Id""
            JOIN public.""Roles"" r ON w.""RolId"" = r.""Id""
            JOIN plan p ON r.""WarehouseId""=p.""WarehouseId""
            JOIN public.""BreakProfiles"" bp ON sc.""BreakProfileId""=bp.""Id""
            JOIN public.""Breaks"" br ON bp.""Id""=br.""BreakProfileId""
            ),

            horas_breaks AS(
                SELECT generate_series(
                        floor(min(br.""InitBreak""))::int,
                        ceiling(max(br.""EndBreak""))::int -1 , 
                        1
                      ) AS hour_break
                FROM breaks br
            ),

            intervals_c_breaks AS (
              SELECT br.""Name"",
                     br.""AvailableWorkerId"",
                     br.workerid,
                      br.workername,
                      hb.hour_break,
                      GREATEST(br.""InitBreak"", hb.hour_break)    AS ""InitBreak"",
                      LEAST(br.""EndBreak"",   hb.hour_break + 1)  AS ""EndBreak""
              FROM breaks br
              JOIN horas_breaks hb
                ON br.""InitBreak"" < (hb.hour_break + 1)
                AND br.""EndBreak""  >= hb.hour_break
            ),

            breaks_lag AS (
                SELECT
                    icb.""Name"",
                    icb.""AvailableWorkerId"",
                    icb.workerid,
                    icb.workername,
                    icb.hour_break,
                    icb.""InitBreak"",
                    icb.""EndBreak"",
                    LAG(icb.""InitBreak"") OVER (PARTITION BY icb.""Name"", icb.""AvailableWorkerId"", icb.hour_break ORDER BY icb.""InitBreak"") AS prev_init,
                    MAX(icb.""EndBreak"") OVER (PARTITION BY icb.""Name"", icb.""AvailableWorkerId"", icb.hour_break ORDER BY icb.""InitBreak"" ROWS BETWEEN UNBOUNDED PRECEDING AND 1 PRECEDING) AS prev_end 
                FROM intervals_c_breaks icb
            ),

            breaks_grouped AS (
                SELECT *,
                    CASE 
                        WHEN prev_end IS NULL THEN 1
                        WHEN ""InitBreak"" > prev_end THEN 1   --No solapan - nuevo grupo
                        ELSE 0                                -- Solapan
                    END AS is_new_group
                FROM breaks_lag
            ),

            breaks_group_id AS (
                SELECT *,
                    SUM(is_new_group) OVER (PARTITION BY ""Name"", ""AvailableWorkerId"", hour_break ORDER BY ""InitBreak"") AS group_id
                FROM breaks_grouped
            ),

            breaks_merged AS (
                SELECT 
                    ""Name"",
                    ""AvailableWorkerId"",
                    workername,
                    workerid,
                    hour_break,
                    MIN(""InitBreak"") AS ""InitBreak"",
                    MAX(""EndBreak"")  AS ""EndBreak""
                FROM breaks_group_id
                GROUP BY 
                    ""Name"", ""AvailableWorkerId"", workername,hour_break, group_id,workerid
            ),

            break_time AS(
                SELECT 
                ib.""Name"",
                ib.""hour_break"",
                (date_trunc('day', now()) + (ib.""hour_break"" * interval '1 hour'))::time AS ""hour_date"",
                ib.workerid,
                ib.workername,
                (date_trunc('day', now()) + (ib.""InitBreak"" * interval '1 hour'))::time AS ""InitBreak"",   
                (date_trunc('day', now()) + (ib.""EndBreak"" * interval '1 hour'))::time AS ""EndBreak""
                FROM breaks_merged ib
            ),

            worksbyhour AS (
                SELECT h.hora,
                        GREATEST(i.""InitDate"", h.hora)                    AS ""InitDate"",
                        LEAST(i.""EndDate"",   h.hora + interval '1 hour')  AS ""EndDate"",
                        i.""ProcessId"",
                        i.""EquipmentGroupId"",
                        i.""WorkerId"",
                        'Operator'::text AS ""ResourceType""
                FROM hours h
                LEFT JOIN itemplanning_utc i
                ON i.""InitDate"" < (h.hora + interval '1 hour')
                AND i.""EndDate""  >  h.hora
            ),

            workbreaksbyhour AS (
                SELECT 
                wb.hora,
                wb.""InitDate"",
                wb.""EndDate"",
                wb.""ProcessId"",
                wb.""EquipmentGroupId"",
                wb.""WorkerId"",
                wb.""ResourceType"",
                CASE 
                WHEN br.""InitBreak"" IS NOT NULL AND br.""EndBreak""  IS NOT NULL
                AND br.""InitBreak"" <= wb.""InitDate""::time
                AND br.""EndBreak"" >= wb.""EndDate""::time
                THEN 1
                ELSE 0
                END AS full_break
                FROM worksbyhour wb
                LEFT JOIN break_time br
                ON br.workerid = wb.""WorkerId""
                AND br.""hour_date""=wb.hora::time
            ),

            warehousebyhour AS (
                SELECT h.hora,
                        GREATEST(w.""InitDate"", h.hora)                    AS ""InitDate"",
                        LEAST(w.""EndDate"",   h.hora + interval '1 hour')  AS ""EndDate"",
                        w.""ProcessId"",
                        w.""EquipmentGroupId"",
                        w.""WorkerId"",
                        'Equipment'::text AS ""ResourceType""
                FROM hours h
                LEFT JOIN wpp_utc w
                ON w.""InitDate"" < (h.hora + interval '1 hour')
                AND w.""EndDate""  >  h.hora
            ),

            warehousebreakbyhour AS(
                SELECT 
                wb.hora,
                wb.""InitDate"",
                wb.""EndDate"",
                wb.""ProcessId"",
                wb.""EquipmentGroupId"",
                wb.""WorkerId"",
                wb. ""ResourceType"",
                CASE 
                WHEN br.""InitBreak"" IS NOT NULL AND br.""EndBreak""  IS NOT NULL
                AND br.""InitBreak"" <= wb.""InitDate""::time
                AND br.""EndBreak"" >= wb.""EndDate""::time
                THEN 1
                ELSE 0
                END AS full_break
                FROM warehousebyhour wb
                LEFT JOIN break_time br
                ON br.workerid = wb.""WorkerId""
                AND br.""hour_date""=wb.hora::time
            ),

            usedbyhour AS (
                SELECT * FROM warehousebreakbyhour wh
                WHERE wh.full_break = 0
                UNION ALL
                SELECT *
                FROM workbreaksbyhour wb
                WHERE wb.full_break = 0
            ),

            workersbyrol AS (
                SELECT w.""RolId"", count(DISTINCT aw.""WorkerId"") AS ""AvailableWorkers""
                FROM public.""Workers"" w
                JOIN public.""AvailableWorkers"" aw ON w.""Id"" = aw.""WorkerId""
                JOIN public.""Roles"" ro ON w.""RolId""=ro.""Id""
                WHERE ro.""WarehouseId""=_warehouse_id
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
                pr.""Name""  AS ""ProcessName"",
                r.""Name""  AS ""ResourceName"",
                r.""Id""
                FROM allinformation ai
                CROSS JOIN (SELECT x.""Name"" FROM public.""Processes"" x JOIN public.""Areas"" a ON x.""AreaId""=a.""Id"" JOIN public.""Layouts"" l ON a.""LayoutId""=l.""Id"" WHERE l.""WarehouseId""=_warehouse_id) pr
                CROSS JOIN (SELECT ro.""Name"",ro.""Id"" FROM public.""Roles"" ro WHERE ro.""WarehouseId""=_warehouse_id) r
            )

            SELECT
                ((r.""When"" AT TIME ZONE 'UTC') + (_utc_offset::interval))::timestamp AS ""When"",
                ((a.""InitDate"" AT TIME ZONE 'UTC') + (_utc_offset::interval))::timestamp AS ""InitDate"",
                ((a.""EndDate"" AT TIME ZONE 'UTC') + (_utc_offset::interval))::timestamp AS ""EndDate"",
                a.""ProcessId"",
                r.""ProcessName"",
                a.""ResourceType"",
                a.""ResourceId"",
                r.""ResourceName"",
                COALESCE(a.""AvailableResources"", wkr.""AvailableWorkers"", 0)::int AS ""AvailableResources""
            FROM resourcesandprocess r
            LEFT JOIN allinformation a
                ON r.""When""        = a.""When""
                AND r.""ProcessName"" = a.""ProcessName""
                AND r.""ResourceName""= a.""ResourceName""
            LEFT JOIN workersbyrol wkr
            ON wkr.""RolId""=r.""Id""
            WHERE r.""When"" IS NOT NULL
            ORDER BY r.""When"", r.""ProcessName"", r.""ResourceName"";
            $function$;
            ;");

            //Labor Function
            migrationBuilder.Sql(@"

            CREATE OR REPLACE FUNCTION public.workforcevsavailabilityview_p(_planning_id uuid, _warehouse_id uuid, _utc_offset interval DEFAULT '00:00:00'::interval, _hour_fmt text DEFAULT 'HH24:MI'::text)
             RETURNS TABLE(""Name"" text, ""When"" timestamp without time zone, workforcevsavailability numeric, worktime numeric, availabilitytime numeric, when_utc_txt text)
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
                      w.""Id"" AS ""WorkerId"",
                      w.""Name"" AS workername,
                      r.""Name""
              FROM plan p
              JOIN public.""WFMLaborWorkerPerProcessType"" wp ON p.""Id"" = wp.""PlanningId""
              JOIN public.""WFMLaborItemPlanning"" ip       ON wp.""Id"" = ip.""WFMLaborPerProcessTypeId""
              JOIN public.""Workers"" w                     ON ip.""WorkerId"" = w.""Id""
              JOIN public.""Roles""   r                     ON w.""RolId"" = r.""Id""
            ),

            workershiftsforlastplanning AS (
              SELECT r.""Name"",
                    sc.""AvailableWorkerId"",
                    w.""Id"" AS ""WorkerId"",
                    sh.""InitHour"",
                    sh.""EndHour"",
                    sh.""WarehouseId""
              FROM public.""Schedules"" sc 
              JOIN public.""Shifts"" sh ON sc.""ShiftId"" = sh.""Id""
              JOIN plan p ON sh.""WarehouseId"" = p.""WarehouseId""
              JOIN public.""AvailableWorkers"" a ON sc.""AvailableWorkerId"" = a.""Id""
              JOIN public.""Workers"" w ON a.""WorkerId"" = w.""Id""
              JOIN public.""Roles"" r ON w.""RolId"" = r.""Id""
            ),

             workershiftsforlastplanning_extra AS ( --Si se trabaja fuera de turno se quiere contar como available time, se asume que solo pasa esto seguido del turno por lo que se amplia con lo que haya en ip
                SELECT
                  w.""Name"",
                  w.""AvailableWorkerId"",
                  w.""WorkerId"",
                  CASE WHEN MIN(ip.""InitDate"") IS NOT NULL THEN
                  (EXTRACT(EPOCH FROM(LEAST(MIN(ip.""InitDate""),date_trunc('day', MIN(ip.""InitDate"")) + (w.""InitHour"" * interval '1 hour'))-date_trunc('day', MIN(ip.""InitDate""))))/3600)::numeric
                  ELSE 
                  w.""InitHour""::numeric
                  END AS ""InitHour"", 

                  CASE WHEN MAX(ip.""EndDate"") IS NOT NULL THEN
                  (EXTRACT(EPOCH FROM (GREATEST(MAX(ip.""EndDate""),date_trunc('day', MAX(ip.""EndDate"")) + (w.""EndHour"" * interval '1 hour'))-date_trunc('day', MAX(ip.""EndDate""))))/3600)::numeric
                  ELSE
                  W.""EndHour""::numeric
                  END AS ""EndHour"",

                  w.""WarehouseId""
              FROM workershiftsforlastplanning w
              LEFT JOIN itemplanning ip
              ON w.""WorkerId""=ip.""WorkerId""
                  GROUP BY 
                    w.""Name"",
                    w.""AvailableWorkerId"",
                    w.""WorkerId"",
                    w.""InitHour"",
                    w.""EndHour"",
                    w.""WarehouseId""
            ),

            horas_roles AS ( --Esto hace falta porque para que el gráfico vaya bien todas los roles deben aparecer en todas las horas. Se hace aquí para hacer más abajo un LEFt JOIN con esto.
              SELECT 
               generate_series(
                floor(MIN(w.""InitHour""))::int,
                ceiling(MAX(w.""EndHour""))::int,
                1) AS hour
              FROM workershiftsforlastplanning_extra w
            ),

            breaks AS(
              SELECT r.""Name"",
                    sc.""AvailableWorkerId"",
                    w.""Id"" AS ""WorkerId"",
                    w.""Name"" AS workername,
                    br.""InitBreak"",
                    br.""EndBreak""
            FROM public.""Schedules"" sc      
            JOIN public.""AvailableWorkers"" a ON sc.""AvailableWorkerId"" = a.""Id""
            JOIN public.""Workers"" w ON a.""WorkerId"" = w.""Id""
            JOIN public.""Roles"" r ON w.""RolId"" = r.""Id""
            JOIN plan p ON r.""WarehouseId""=p.""WarehouseId""
            JOIN public.""BreakProfiles"" bp ON sc.""BreakProfileId""=bp.""Id""
            JOIN public.""Breaks"" br ON bp.""Id""=br.""BreakProfileId""
            ),

            horas_breaks AS(
                SELECT generate_series(
                        floor(min(br.""InitBreak""))::int,
                        ceiling(max(br.""EndBreak""))::int -1 , 
                        1
                      ) AS hour_break
                FROM breaks br
            ),

            intervals_c_breaks AS (
              SELECT br.""Name"",
                     br.""AvailableWorkerId"",
                      br.workername,
                      hb.hour_break,
                      GREATEST(br.""InitBreak"", hb.hour_break)    AS ""InitBreak"",
                      LEAST(br.""EndBreak"",   hb.hour_break + 1)  AS ""EndBreak""
              FROM breaks br
              JOIN horas_breaks hb
                ON br.""InitBreak"" < (hb.hour_break + 1)
                AND br.""EndBreak""  >= hb.hour_break
            ),

            breaks_lag AS (
                SELECT
                    icb.""Name"",
                    icb.""AvailableWorkerId"",
                    icb.workername,
                    icb.hour_break,
                    icb.""InitBreak"",
                    icb.""EndBreak"",
                    LAG(icb.""InitBreak"") OVER (PARTITION BY icb.""Name"", icb.""AvailableWorkerId"", icb.hour_break ORDER BY icb.""InitBreak"") AS prev_init,
                    MAX(icb.""EndBreak"") OVER (PARTITION BY icb.""Name"", icb.""AvailableWorkerId"", icb.hour_break ORDER BY icb.""InitBreak"" ROWS BETWEEN UNBOUNDED PRECEDING AND 1 PRECEDING) AS prev_end 
                FROM intervals_c_breaks icb
            ),

            breaks_grouped AS (
                SELECT *,
                    CASE 
                        WHEN prev_end IS NULL THEN 1
                        WHEN ""InitBreak"" > prev_end THEN 1   --No solapan - nuevo grupo
                        ELSE 0                                -- Solapan
                    END AS is_new_group
                FROM breaks_lag
            ),

            breaks_group_id AS (
                SELECT *,
                    SUM(is_new_group) OVER (PARTITION BY ""Name"", ""AvailableWorkerId"", hour_break ORDER BY ""InitBreak"") AS group_id
                FROM breaks_grouped
            ),

            breaks_merged AS ( --breaks en float
                SELECT 
                    ""Name"",
                    ""AvailableWorkerId"",
                    workername,
                    hour_break,
                    MIN(""InitBreak"") AS ""InitBreak"",
                    MAX(""EndBreak"")  AS ""EndBreak""
                FROM breaks_group_id
                GROUP BY 
                    ""Name"", ""AvailableWorkerId"", workername,hour_break, group_id
            ),

            break_time AS( --Los shifts y los breaks deben ser time para poder compararlos con fechas de distinto dia
                SELECT 
                ib.""Name"",
                ib.""hour_break"",
                ib.workername,
                (date_trunc('day', now()) + (ib.""InitBreak"" * interval '1 hour'))::time AS ""InitBreak"",    
                (date_trunc('day', now()) + (ib.""EndBreak"" * interval '1 hour'))::time AS ""EndBreak""
                FROM breaks_merged ib
            ),

            horasworkershifts AS (  --Por cada turno genera un grid con las horas (gs) y va comprobando para cada hora que parte del turno cae dentro, restando el break que se solape)
            SELECT
                w.""AvailableWorkerId"",
                w.""Name"",
                gs.hour AS hour_int,
                GREATEST(0,
                CASE
                  WHEN w.""InitHour"" >= gs.hour AND w.""EndHour"" <= gs.hour + 1 THEN (w.""EndHour"" - w.""InitHour"") -- Caso: turno empieza y termina dentro de la misma hora

                  WHEN gs.hour >= w.""InitHour"" AND gs.hour + 1 <= w.""EndHour"" THEN 1                   -- Si la hora está completamente dentro del turno

                  WHEN w.""InitHour"" >= gs.hour AND w.""InitHour"" < gs.hour + 1 THEN (gs.hour + 1 - w.""InitHour"")       -- Si el turno empieza dentro de esta hora

                  WHEN w.""EndHour"" > gs.hour AND w.""EndHour"" < gs.hour + 1 THEN (w.""EndHour"" - gs.hour)                 -- Si el turno termina dentro de esta hora

                  ELSE 0
                END 
                -SUM(CASE WHEN breaks.""InitBreak"" IS NOT NULL AND breaks.""EndBreak"" IS NOT NULL THEN-- Restar las partes de los breaks que coincidan con el turno
                    GREATEST(0,LEAST(w.""EndHour"", breaks.""EndBreak"") - GREATEST(w.""InitHour"", breaks.""InitBreak""))
                    ELSE 0 
                END)) AS full_hour
                FROM workershiftsforlastplanning_extra w
                CROSS JOIN horas_roles  AS gs
                LEFT JOIN breaks_merged breaks
                ON w.""AvailableWorkerId"" = breaks.""AvailableWorkerId"" 
                AND gs.hour = breaks.hour_break
                GROUP BY w.""AvailableWorkerId"",w.""Name"",gs.hour,w.""InitHour"",w.""EndHour""
            ),

            available_hours AS (
              SELECT
                worker_hour.""Name"",
                worker_hour.hour_int,
                SUM(worker_hour.full_hour) * 3600 AS availabilitytime --Se suman las horas availables de los workers, si no es entera se sumara la parte proporcional
              FROM (
                SELECT
                  hw.""AvailableWorkerId"",
                  hw.""Name"",
                  hw.hour_int,
                  LEAST(SUM(hw.full_hour),1) AS full_hour     --Esto asegura que para cada trabajador y por hora como máximo se coge una hora como available (por si se superpsuieran trabajos)
                FROM horasworkershifts hw
                GROUP BY hw.""AvailableWorkerId"", hw.""Name"", hw.hour_int
              ) AS worker_hour
              GROUP BY worker_hour.""Name"", worker_hour.hour_int
            ),


            limits AS (
              SELECT 
                date_trunc('hour', min(itemplanning.""InitDate"")) - interval '1 hour' AS firsthour,
                date_trunc('hour', max(itemplanning.""EndDate"")) + interval '1 hour' AS lasthour
              FROM itemplanning
            ),
            horas AS (
              SELECT generate_series(
                        (SELECT firsthour FROM limits),
                        (SELECT lasthour  FROM limits), 
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
                      lag(ic.""InitDate"") OVER (PARTITION BY ic.""Name"",ic.workername, ic.hour ORDER BY ic.""InitDate"") AS prev_init_date,
                      max(ic.""EndDate"")  OVER (PARTITION BY ic.""Name"",ic.workername, ic.hour ORDER BY ic.""InitDate"" ROWS BETWEEN UNBOUNDED PRECEDING AND 1 PRECEDING) AS prev_end_date
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
                      iwg.prev_init_date,
                      iwg.prev_end_date,
                      iwg.is_new_group,
                      sum(iwg.is_new_group) OVER (PARTITION BY iwg.""Name"", iwg.hour, workername ORDER BY iwg.""InitDate"") AS grupo
              FROM intervals_withgroups iwg
            ),


            intervals_merged AS (
              SELECT ig.""Name"",
                      ig.workername,
                      ig.hour,
                      date_part('hour', ig.hour)::int AS hour_int,
                      min(ig.""InitDate"") AS ""InitDate"",
                      max(ig.""EndDate"")  AS ""EndDate""
              FROM intervals_groups ig
              GROUP BY ig.""Name"", ig.workername, ig.hour, ig.grupo
            ),

            intervals_merged_breaks AS (
              SELECT ig.""Name"",
                      ig.workername,
                      ig.hour,
                     (ig.""EndDate"" - ig.""InitDate"") AS worktime,
                     SUM(CASE WHEN br.""InitBreak"" IS NOT NULL AND br.""EndBreak"" IS NOT NULL THEN
                      GREATEST(INTERVAL '0 seconds',LEAST(ig.""EndDate""::time, br.""EndBreak""::time) - GREATEST(ig.""InitDate""::time, br.""InitBreak""::time))
                     ELSE INTERVAL '0 seconds'
                     END) AS breaktime
              FROM intervals_merged ig
              LEFT JOIN break_time br
              ON ig.workername=br.workername
              AND ig.""hour_int""=br.""hour_break""
              AND ig.""Name""=br.""Name"" -- Esto es supuestamente redundante
              GROUP BY ig.""Name"", ig.workername, ig.hour,ig.""InitDate"",ig.""EndDate""
            ),

            worktime_utc AS (
              SELECT 
                im.""Name"",
                im.hour AS when_utc,
                SUM(EXTRACT(epoch FROM (im.worktime -im.breaktime)))::numeric AS worktime 
              FROM intervals_merged_breaks im
              GROUP BY im.""Name"", im.hour
              ),


            limits_roles AS (
              SELECT
              h.hour AS when_utc,
              date_part('hour', h.hour)::int AS hour_int,
              r.""Name""
              FROM horas h
              CROSS JOIN (SELECT ro.""Name"" FROM public.""Roles"" ro WHERE ro.""WarehouseId""=_warehouse_id) r
            ),

            base AS (
              SELECT lr.""Name"",
                    lr.when_utc,
                    COALESCE(wt.worktime, 0::numeric) AS worktime,
                    COALESCE(s.availabilitytime, 0::numeric) AS availabilitytime
                    FROM limits_roles lr
                    LEFT JOIN worktime_utc wt
                    ON lr.""Name""=wt.""Name""
                    AND lr.when_utc=wt.when_utc
                    LEFT JOIN available_hours s
                      ON lr.""Name"" = s.""Name""
                      AND lr.hour_int = s.hour_int
            )
            SELECT
              b.""Name"",
              ((b.when_utc AT TIME ZONE 'UTC') + _utc_offset) AS ""When"",
              CASE                                                                            --Se disitnguen casos para cuando se trabaja fuera de turno
              WHEN b.availabilitytime=0 AND b.worktime>0 THEN 1
              --WHEN b.availabilitytime=0 AND b.worktime=0 THEN 0
              WHEN b.availabilitytime>0 THEN LEAST(b.worktime::numeric / b.availabilitytime::numeric,1) --Least por seguridad, aunque esto no ocurre con los calculos que se hacen
              ELSE 0
              END AS workforcevsavailability,
              b.worktime,
              b.availabilitytime,
              to_char(date_trunc('hour', (b.when_utc AT TIME ZONE 'UTC') + _utc_offset), _hour_fmt) AS when_utc_txt
            FROM base b
            ORDER BY b.""Name"", ""When"";
            $function$;
            ");



        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS public.areaview_p(uuid, uuid, text, text);");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS public.resourcesusageview_p(uuid, uuid, interval, text);");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS public.planningresourcesview_p(uuid, uuid, text, text);");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS public.workforcevsavailabilityview_p(uuid, uuid, interval, text);");


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
;


            ");

            //Planning Function
            migrationBuilder.Sql(@"

            CREATE OR REPLACE FUNCTION public.planningresourcesview_p(_planning_id uuid, _utc_offset text, _hour_format text)
             RETURNS TABLE(""When"" timestamp without time zone, ""InitDate"" timestamp without time zone, ""EndDate"" timestamp without time zone, ""ProcessId"" uuid, ""ProcessName"" text, ""ResourceType"" text, ""ResourceId"" uuid, ""ResourceName"" text, ""AvailableResources"" integer)
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
            $function$
            ;

            ");

            //Resource usage
            migrationBuilder.Sql(@"

            CREATE OR REPLACE FUNCTION public.resourcesusageview_p(_planning_id uuid, _utc_offset interval DEFAULT '00:00:00'::interval, _hour_fmt text DEFAULT 'HH24:MI'::text)
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

            $function$;

            ");


            //Labor Function 
            migrationBuilder.Sql(@"

            CREATE OR REPLACE FUNCTION public.workforcevsavailabilityview_p(_planning_id uuid, _utc_offset interval DEFAULT '00:00:00'::interval, _hour_fmt text DEFAULT 'HH24:MI'::text)
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
            $function$;

            ");


        }
    }
}
