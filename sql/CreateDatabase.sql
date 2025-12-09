-- Crear la base de datos
CREATE DATABASE [BOTSwapper];
GO


-- Usar la base de datos
USE [BOTSwapper];
GO
IF OBJECT_ID('dbo.IOLTokens') IS NOT NULL
BEGIN
    DROP TABLE dbo.IOLTokens
END
GO

IF OBJECT_ID('dbo.Logs') IS NOT NULL
BEGIN
    DROP TABLE dbo.Logs
END

GO

IF OBJECT_ID('dbo.MD_DATA') IS NOT NULL
BEGIN
    DROP TABLE dbo.MD_DATA
END
GO

CREATE TABLE [dbo].[MD_DATA](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[DT] [datetime] NOT NULL,
	[Bono1_Bid] [float] NOT NULL,
	[Bono1_Last] [float] NOT NULL,
	[Bono1_Ask] [float] NOT NULL,
	[Bono2_Bid] [float] NOT NULL,
	[Bono2_Last] [float] NOT NULL,
	[Bono2_Ask] [float] NOT NULL,
	[Ratio]  AS ([Bono1_Last]/nullif([Bono2_Last],(0))),
	[MM180] [float] NULL,
	[GDAL]  AS ([Bono1_Last]/nullif([Bono2_Ask],(0))),
	[ALGD]  AS ([Bono2_Last]/nullif([Bono1_Ask],(0))),
	[Bono1] varchar(50) NOT NULL,
	[Bono2] varchar(50) NOT NULL
) 

GO

CREATE OR ALTER PROCEDURE [dbo].[sp_MD_INS]
    @dt DATETIME,
    @Bono1_Bid FLOAT,
    @Bono1_Last FLOAT,
    @Bono1_Ask FLOAT,
    @Bono2_Bid FLOAT,
    @Bono2_Last FLOAT,
    @Bono2_Ask FLOAT,
	@Bono1 VARCHAR(50),
	@Bono2 VARCHAR(50)
AS
BEGIN
    --NO REPETIR
	IF EXISTS(SELECT TOP 1 1 FROM dbo.MD_DATA WHERE
		Bono1_Bid = @Bono1_Bid AND
		Bono1_Last = @Bono1_Last AND
		Bono1_Ask = @Bono1_Ask AND
		Bono2_Bid = @Bono2_Bid AND 
		Bono2_Last = @Bono2_Last AND 
		Bono2_Ask = @Bono2_Ask AND
		Bono1 = @Bono1 AND 
		Bono2 = @Bono2
	)
		RETURN;

	declare @id int

    INSERT INTO dbo.MD_DATA (DT, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, Bono1, Bono2)
    VALUES (@dt, @Bono1_Bid, @Bono1_Last, @Bono1_Ask, @Bono2_Bid, @Bono2_Last, @Bono2_Ask, @Bono1, @Bono2);
		
	SELECT @id = SCOPE_IDENTITY();

	UPDATE MD_DATA
	SET MM180 = (
		SELECT AVG(Ratio)
		FROM (
			SELECT TOP (180) Ratio
			FROM MD_DATA
			WHERE DT <= @dt
			ORDER BY DT DESC
		) X
	)
	WHERE ID = @id;

END
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_GetDataSet]
    @dt DATETIME,
	@bono1 varchar(50),
	@bono2 varchar(50)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 5000
        DT,
        Ratio,
        MM180,
        GDAL,
        ALGD
    FROM dbo.MD_DATA
    WHERE DT <= @dt and Bono1 = @bono1 and Bono2 = @bono2
    ORDER BY DT ASC;
END

GO

CREATE OR ALTER PROCEDURE [dbo].[sp_GetData]
    @dt DATETIME,
	@bono1 varchar(50),
	@bono2 varchar(50)
AS
BEGIN
    SET NOCOUNT ON;

    ;WITH Ultimos AS
    (
        SELECT TOP (180)
            DT, Ratio, GDAL, ALGD
        FROM dbo.MD_DATA
        WHERE DT <= @dt and Bono1 = @bono1 and Bono2 = @bono2
        ORDER BY DT DESC
    )
    SELECT 
        MAX(DT) AS DT,
        MAX(Ratio) AS Ratio,
        AVG(Ratio) AS MM180,
        MAX(GDAL) AS GDAL,
        MAX(ALGD) AS ALGD,
        MIN(Ratio) AS Piso,
        MAX(Ratio) AS Techo,
        STDEV(Ratio) AS Desvio,
        VAR(Ratio) AS Volatilidad
    FROM Ultimos;
END
GO





/*
select 'insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES (''' 
+ CONVERT(varchar(23), id, 126) + ''', '''  
+ CONVERT(varchar(23), dt, 126) + ''', '''  
+ convert(varchar,Bono1_Bid) + ''',' + ''''
+ convert(varchar,Bono1_Last) + ''',' + ''''
+ convert(varchar,Bono1_Ask) + ''',' + ''''
+ convert(varchar,Bono2_Bid) + ''',' + ''''
+ convert(varchar,Bono2_Last) + ''',' + ''''
+ convert(varchar,Bono2_Ask) + ''',' + ''''
+ convert(varchar,MM180) + ''',' + ''''
+ convert(varchar,Bono1) + ''',' + ''''
+ convert(varchar,Bono2) + '''' + ')'
from MD_DATA
*/

SET IDENTITY_INSERT MD_DATA ON;

insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('1', '2025-05-02T16:00:00', '80900','81200','81500','79200','79500','79800','1.02138','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('2', '2025-05-05T16:00:00', '81050','81350','81650','79350','79650','79950','1.02136','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('3', '2025-05-06T16:00:00', '81100','81400','81700','79400','79700','80000','1.02135','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('4', '2025-05-07T16:00:00', '81150','81450','81750','79450','79750','80050','1.02134','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('5', '2025-05-08T16:00:00', '81200','81500','81800','79500','79800','80100','1.02134','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('6', '2025-05-09T16:00:00', '81250','81550','81850','79550','79850','80150','1.02133','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('7', '2025-05-12T16:00:00', '81300','81600','81900','78200','78500','78800','1.02392','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('8', '2025-05-13T16:00:00', '81350','81650','81950','78600','78900','79200','1.02529','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('9', '2025-05-14T16:00:00', '81400','81700','82000','79000','79300','79600','1.02584','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('10', '2025-05-15T16:00:00', '81450','81750','82050','79100','79400','79700','1.02622','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('11', '2025-05-16T16:00:00', '81500','81800','82100','79200','79500','79800','1.02646','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('12', '2025-05-19T16:00:00', '81550','81850','82150','79600','79900','80200','1.02629','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('13', '2025-05-20T16:00:00', '81600','81900','82200','79700','80000','80300','1.0261','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('14', '2025-05-21T16:00:00', '81650','81950','82250','79750','80050','80350','1.02593','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('15', '2025-05-22T16:00:00', '81700','82000','82300','79800','80100','80400','1.02578','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('16', '2025-05-23T16:00:00', '81750','82050','82350','79850','80150','80450','1.02565','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('17', '2025-05-26T16:00:00', '81800','82100','82400','79900','80200','80500','1.02554','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('18', '2025-05-27T16:00:00', '81850','82150','82450','79950','80250','80550','1.02543','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('19', '2025-05-28T16:00:00', '81900','82200','82500','80000','80300','80600','1.02534','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('20', '2025-05-29T16:00:00', '81950','82250','82550','80050','80350','80650','1.02525','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('21', '2025-05-30T16:00:00', '82000','82300','82600','80100','80400','80700','1.02518','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('22', '2025-06-02T16:00:00', '82450','82880','83650','80670','80710','81450','1.02526','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('23', '2025-06-03T16:00:00', '82600','83190','83450','80250','81100','81320','1.02528','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('24', '2025-06-04T16:00:00', '82460','83100','83760','80730','80900','81190','1.02536','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('25', '2025-06-05T16:00:00', '81750','82350','83950','80010','80080','81150','1.02548','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('26', '2025-06-06T16:00:00', '81220','82640','82750','80000','80630','80700','1.02546','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('27', '2025-06-09T16:00:00', '81600','83200','83290','80540','81150','81270','1.02545','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('28', '2025-06-10T16:00:00', '83250','84100','84190','80950','81750','81930','1.02557','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('29', '2025-06-11T16:00:00', '83400','83510','84740','81170','81210','82070','1.02566','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('30', '2025-06-12T16:00:00', '82840','83500','83860','80900','81240','81550','1.02573','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('31', '2025-06-13T16:00:00', '83180','83270','84200','80980','81050','81470','1.02579','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('32', '2025-06-16T16:00:00', '83300','83400','84300','81000','81300','81600','1.02579','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('33', '2025-06-17T16:00:00', '83350','83550','84450','79250','79400','80540','1.02659','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('34', '2025-06-18T16:00:00', '83400','83600','84500','77950','78100','79110','1.02788','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('35', '2025-06-19T16:00:00', '83450','83750','84650','78100','78930','79440','1.02883','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('36', '2025-06-20T16:00:00', '83500','83800','84700','78500','79200','79700','1.02964','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('37', '2025-06-23T16:00:00', '83550','83950','84850','78500','80590','81000','1.02997','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('38', '2025-06-24T16:00:00', '83600','84050','84950','80810','81730','82000','1.02992','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('39', '2025-06-25T16:00:00', '83650','84150','85050','80980','82350','82370','1.02972','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('40', '2025-06-26T16:00:00', '83700','84250','85150','82380','82990','83070','1.02935','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('41', '2025-06-27T16:00:00', '83750','84300','85250','82200','82750','83100','1.0291','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('42', '2025-06-30T16:00:00', '83800','84350','85300','82300','82800','83150','1.02885','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('43', '2025-07-01T16:00:00', '82580','84350','84410','82580','84350','84410','1.02818','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('44', '2025-07-02T16:00:00', '84080','85290','85290','84080','85290','85290','1.02754','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('45', '2025-07-03T16:00:00', '84650','85200','85650','84650','85200','85650','1.02693','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('46', '2025-07-04T16:00:00', '85140','85900','86500','85140','85900','86500','1.02634','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('47', '2025-07-07T16:00:00', '85510','86900','87250','85510','86900','87250','1.02578','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('48', '2025-07-08T16:00:00', '77250','77310','78590','86000','87000','87500','1.02292','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('49', '2025-07-09T16:00:00', '76860','78600','78640','86500','87200','87700','1.02044','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('50', '2025-07-10T16:00:00', '77340','77600','78780','87000','87600','88100','1.01775','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('51', '2025-07-11T16:00:00', '77500','77600','78780','87200','87800','88300','1.01512','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('52', '2025-07-14T16:00:00', '77190','78830','78830','87500','88000','88500','1.01283','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('53', '2025-07-15T16:00:00', '77590','77880','79630','87700','88200','88700','1.01038','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('54', '2025-07-16T16:00:00', '76410','77050','77690','87900','88400','88900','1.00781','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('55', '2025-07-17T16:00:00', '76610','77150','77770','88100','88600','89100','1.00532','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('56', '2025-07-18T16:00:00', '77500','78360','78490','88300','88800','89300','1.00312','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('57', '2025-07-21T16:00:00', '77630','77750','78900','88500','89000','89500','1.00085','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('58', '2025-07-22T16:00:00', '75800','75800','78190','88700','89200','89700','0.998246','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('59', '2025-07-23T16:00:00', '76020','76450','77500','88900','89400','89900','0.995821','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('60', '2025-07-24T16:00:00', '76380','78000','78340','89100','89600','90100','0.993732','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('61', '2025-07-25T16:00:00', '77620','79500','79680','89300','89800','90300','0.991955','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('62', '2025-07-28T16:00:00', '79390','80150','80330','89500','90000','90500','0.990319','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('63', '2025-07-29T16:00:00', '79500','79730','81330','89700','90200','90700','0.988631','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('64', '2025-07-30T16:00:00', '79730','82250','82250','89900','90400','90900','0.9874','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('65', '2025-07-31T16:00:00', '82250','84250','84770','90100','90600','91100','0.986515','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('66', '2025-08-01T16:00:00', '74000','74250','74500','70800','71050','71350','0.987402','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('67', '2025-08-04T16:00:00', '74100','74350','74650','70900','71150','71450','0.988261','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('68', '2025-08-05T16:00:00', '74200','74450','74750','71000','71250','71550','0.989094','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('69', '2025-08-06T16:00:00', '74300','74550','74850','71100','71350','71650','0.989902','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('70', '2025-08-07T16:00:00', '74400','74650','74950','71200','71450','71750','0.990686','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('71', '2025-08-08T16:00:00', '74500','74750','75050','71300','71550','71850','0.991447','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('72', '2025-08-11T16:00:00', '74600','74850','75150','71400','71650','71950','0.992187','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('73', '2025-08-12T16:00:00', '74700','74950','75250','71500','71750','72050','0.992905','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('74', '2025-08-13T16:00:00', '74800','75050','75350','71600','71850','72150','0.993602','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('75', '2025-08-14T16:00:00', '74900','75150','75450','71700','71950','72250','0.994281','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('76', '2025-08-15T16:00:00', '75000','75250','75550','71800','72050','72350','0.99494','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('77', '2025-08-18T16:00:00', '75100','75350','75650','71900','72150','72450','0.995582','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('78', '2025-08-19T16:00:00', '75200','75450','75750','72000','72250','72550','0.996206','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('79', '2025-08-20T16:00:00', '75300','75550','75850','72100','72350','72650','0.996814','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('80', '2025-08-21T16:00:00', '75400','75650','75950','72200','72450','72750','0.997406','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('81', '2025-08-22T16:00:00', '75500','75750','76050','72300','72550','72850','0.997983','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('82', '2025-08-25T16:00:00', '75600','75850','76150','72400','72650','72950','0.998545','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('83', '2025-08-26T16:00:00', '75700','75950','76250','72500','72750','73050','0.999092','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('84', '2025-08-27T16:00:00', '75800','76050','76350','72600','72850','73150','0.999626','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('85', '2025-08-28T16:00:00', '75900','76150','76450','72700','72950','73250','1.00015','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('86', '2025-08-29T16:00:00', '76000','76250','76550','72800','73050','73350','1.00065','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('87', '2025-09-01T16:00:00', '76000','76250','76500','72500','72800','73100','1.00119','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('88', '2025-09-02T16:00:00', '75900','76150','76400','72400','72650','72950','1.00173','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('89', '2025-09-03T16:00:00', '75800','76000','76300','72300','72550','72850','1.00224','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('90', '2025-09-04T16:00:00', '75700','75950','76250','72200','72450','72750','1.00275','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('91', '2025-09-05T16:00:00', '75600','75800','76100','72100','72350','72650','1.00325','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('92', '2025-09-08T16:00:00', '75500','75750','76050','72000','72250','72550','1.00374','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('93', '2025-09-09T16:00:00', '75400','75650','75950','71900','72150','72450','1.00422','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('94', '2025-09-10T16:00:00', '75300','75500','75800','71800','72050','72350','1.00468','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('95', '2025-09-11T16:00:00', '75200','75450','75750','71700','71950','72250','1.00515','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('96', '2025-09-12T16:00:00', '75100','75300','75600','71600','71850','72150','1.00559','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('97', '2025-09-15T16:00:00', '75000','75250','75550','71500','71750','72050','1.00604','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('98', '2025-09-16T16:00:00', '74900','75100','75400','71400','71650','71950','1.00647','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('99', '2025-09-17T16:00:00', '74800','75050','75350','71300','71550','71850','1.0069','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('100', '2025-09-18T16:00:00', '74700','74900','75200','71200','71450','71750','1.00731','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('101', '2025-09-19T16:00:00', '74600','74850','75150','71100','71350','71650','1.00772','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('102', '2025-09-22T16:00:00', '74500','74700','75000','71000','71250','71550','1.00812','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('103', '2025-09-23T16:00:00', '74400','74650','74950','70900','71150','71450','1.00852','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('104', '2025-09-24T16:00:00', '74300','74500','74800','70800','71050','71350','1.00891','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('105', '2025-09-25T16:00:00', '74200','74450','74750','70700','70950','71250','1.00929','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('106', '2025-09-26T16:00:00', '74100','74300','74600','70600','70850','71150','1.00966','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('107', '2025-09-29T16:00:00', '74000','74250','74550','70500','70750','71050','1.01003','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('108', '2025-09-30T16:00:00', '73900','74100','74400','70400','70650','70950','1.01039','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('109', '2025-10-01T16:00:00', '80330','82130','85740','78000','78500','81000','1.01072','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('110', '2025-10-02T16:00:00', '81040','85790','86130','79500','80200','82500','1.01126','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('111', '2025-10-03T16:00:00', '84830','87400','87990','81000','82000','84000','1.01175','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('112', '2025-10-06T16:00:00', '87000','88300','89900','83000','83500','85500','1.01216','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('113', '2025-10-07T16:00:00', '87050','88340','89370','83500','84000','86000','1.01251','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('114', '2025-10-08T16:00:00', '86200','88360','88820','84000','84500','86500','1.0128','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('115', '2025-10-09T16:00:00', '87500','88020','91860','84500','85000','87000','1.013','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('116', '2025-10-13T16:00:00', '80030','88350','88660','82000','83000','85000','1.01344','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('117', '2025-10-14T16:00:00', '81590','86980','88960','83000','84000','86000','1.01363','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('118', '2025-10-15T16:00:00', '85040','86200','88450','84000','84500','86500','1.01368','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('119', '2025-10-16T16:00:00', '85720','87360','88210','84500','85000','87000','1.0138','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('120', '2025-10-17T16:00:00', '86270','88410','88980','85000','85500','87500','1.01397','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('121', '2025-10-20T16:00:00', '87350','91610','92000','85500','86000','88000','1.01439','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('122', '2025-10-21T16:00:00', '90960','93600','94100','87410','88730','89480','1.01473','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('123', '2025-10-22T16:00:00', '91000','92250','94500','86210','86340','88900','1.01516','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('124', '2025-10-23T16:00:00', '87030','89000','93040','83260','83510','87000','1.01557','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('125', '2025-10-24T16:00:00', '87000','88550','90330','82350','83670','84450','1.01591','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('126', '2025-10-27T16:00:00', '88510','93100','100000','85700','89430','91000','1.01611','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('127', '2025-10-28T16:00:00', '93100','97310','97820','89610','93150','94000','1.01634','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('128', '2025-10-29T16:00:00', '96010','97160','97680','90950','93130','93700','1.01655','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('129', '2025-10-30T16:00:00', '96910','97530','98430','92500','93530','93700','1.01675','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('130', '2025-10-31T16:00:00', '97140','98450','99340','93550','94410','94750','1.01695','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('131', '2025-11-03T16:00:00', '96800','96950','97000','93600','93750','93800','1.01708','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('132', '2025-11-04T16:00:00', '96900','97050','97100','94500','94650','94700','1.01715','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('133', '2025-11-05T16:00:00', '96950','97100','97150','94200','94350','94400','1.01724','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('134', '2025-11-06T16:00:00', '97000','97150','97200','94000','94150','94200','1.01734','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('135', '2025-11-07T16:00:00', '95800','95950','96000','92500','92650','92700','1.01748','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('136', '2025-11-10T16:00:00', '97300','97450','97500','94100','94250','94300','1.0176','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('137', '2025-11-11T16:00:00', '97500','97650','97700','94300','94450','94500','1.01772','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('138', '2025-11-12T16:00:00', '97300','97450','97500','93800','93950','94000','1.01786','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('139', '2025-11-13T16:00:00', '97000','97150','97200','93200','93350','93400','1.01803','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('140', '2025-11-14T16:00:00', '97200','97350','97400','93700','93850','93900','1.01816','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('141', '2025-11-17T16:00:00', '97350','97500','97550','93800','93950','94000','1.0183','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('142', '2025-11-18T16:00:00', '96500','96650','96700','92900','93050','93100','1.01845','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('143', '2025-11-19T16:00:00', '97200','97350','97400','93600','93750','93800','1.01859','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('144', '2025-11-20T16:00:00', '97600','97750','97800','94100','94250','94300','1.01871','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('145', '2025-11-21T16:00:00', '97800','97950','98000','94400','94550','94600','1.01883','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('146', '2025-11-24T16:00:00', '98000','98150','98200','94800','94950','95000','1.01894','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('147', '2025-11-25T16:00:00', '98700','98850','98900','95500','95650','95700','1.01903','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('148', '2025-11-26T16:00:00', '98800','98950','99000','95400','95550','95600','1.01915','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('149', '2025-11-27T16:00:00', '99000','99150','99200','95800','95950','96000','1.01924','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('150', '2025-11-28T16:00:00', '98500','98650','98700','95200','95350','95400','1.01934','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('151', '2025-12-01T16:00:00', '99920','100210','100500','94980','95300','95620','1.01956','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('152', '2025-12-02T16:00:00', '97500','97520','97550','95500','95520','95540','1.01957','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('153', '2025-12-03T16:00:00', '97600','97630','97660','95600','95625','95645','1.01958','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('154', '2025-12-04T16:00:00', '97700','97740','97770','95700','95730','95750','1.01958','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('155', '2025-12-05T16:00:00', '97800','97850','97880','95800','95840','95860','1.01959','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('156', '2025-12-06T16:00:00', '97850','97900','97930','95850','95890','95910','1.0196','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('157', '2025-12-07T16:00:00', '97900','97950','97980','95900','95940','95960','1.01961','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('158', '2025-12-08T16:00:00', '97950','98000','98030','95950','95990','96010','1.01962','GD30','AL30')
insert into MD_DATA (Id, dt, Bono1_Bid, Bono1_Last, Bono1_Ask, Bono2_Bid, Bono2_Last, Bono2_Ask, MM180, Bono1, Bono2) VALUES ('159', '2025-12-09T12:00:00', '98000','98050','98080','96000','96040','96060','1.01963','GD30','AL30')

SET IDENTITY_INSERT MD_DATA OFF;