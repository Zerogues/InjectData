USE [EDU2025]
GO
/****** Object:  StoredProcedure [dbo].[add_xml_data]    Script Date: 09.02.2025 22:11:34 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[add_xml_data]
	@Institution int
	, @EducationForm int
	, @DataAll xml
	, @FilePath varchar(255)
AS
BEGIN
	DECLARE @Data xml = @DataAll.query('/*')
	INSERT INTO education_plan (id_institution, id_education_form, data, file_path)
	VALUES (@Institution, @EducationForm, @Data, @FilePath)
	
END;
GO
/****** Object:  StoredProcedure [dbo].[dublicate_check]    Script Date: 09.02.2025 22:11:34 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*DECLARE @FullXml xml;
SELECT @FullXml = BulkColumn
FROM openrowset (BULK 'D:\khsu\Планы\ИТИ\ОФО\09.03.01_ИВТ_ПОВТиАС_3к_2024.plx', SINGLE_BLOB) AS x

EXEC add_xml_data
	@Institution = 11
	, @EducationForm = 1
	, @DataAll = @FullXml
	,	@FilePath = 'Планы\ИТИ\ОФО\09.03.01_ИВТ_ПОВТиАС_3к_2024.plx';
*/

CREATE PROCEDURE [dbo].[dublicate_check]
	@Institution int
	, @EducationForm int
	, @FilePath varchar(255)
AS
BEGIN
	IF EXISTS (
		SELECT 1
		FROM education_plan
		WHERE id_institution = @Institution
		AND id_education_form = @EducationForm
		AND file_path = @FilePath
	)
	BEGIN
		SELECT id FROM education_plan WHERE file_path = @FilePath
		PRINT 'Запись уже существует. ';
	END
	ELSE
	BEGIN
		PRINT 'Запись не существует.';
	END;
END;
GO
