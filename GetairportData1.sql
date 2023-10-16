SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE procedure [dbo].[GetairportData1]
@countn int,
@icao nvarchar(16),
@sort nvarchar(16),
@city nvarchar(max),
@country nvarchar(max)

as
begin
    create table #tempdb
    (
        icao varchar(10),
        city varchar(100),
        country varchar(100),
        rwy int,
        elevation float,
        location GEOGRAPHY
    )
    declare @sqlstr NVARCHAR(max)

    set @sqlstr = 'insert into #tempdb '
    + 'select top (' + cast(@countn as nvarchar) + ') airportlist2.icao, airportlist2.City, '
    + 'airportlist2.country, airportlist2.rwy, airportlist2.elevation, airportlist2.location '
    + 'from airportlist2 '
    + 'where airportlist2.icao like ''' + @icao +'%'' AND airportlist2.city like '''
    + @city + '%'' AND airportlist2.country like '''
    + @country + '%'' '
    + 'order by ' + @sort +';'

    exec sp_executesql @sqlstr

    select 
	((
		select
			geography::UnionAggregate(location).ToString() as airpdata,
        
            (select
			    icao as 'icao',
                city as 'city',
                country as 'country',
                convert (int, rwy) as 'rwy',
                convert(int, elevation) as 'elevation',
                location.ToString() as 'location'
            from #tempdb
            for json path
            ) as 'aps'
	    from #tempdb
	    for json path, without_array_wrapper
	)) as locationData
end
GO
