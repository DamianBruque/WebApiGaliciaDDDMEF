﻿

--SELECT DOCUMENTO.DOCUMENTNUMBER,DOCUMENTO.DOCUMENTTYPE,DATAPOLICY.EFFECTIVEDATE
--FROM UNDERWRITING.UNDERWRITINGCASE UC
--    INNER JOIN UNDERWRITING.UNDERWRITINGCASERISK UR ON 
--       UC.UNDERWRITINGCASEID = UR.UNDERWRITINGCASEID,
--        (XMLTABLE('RiskInformation/Roles/Role/Client/ClientDocuments/ClientDocument' PASSING UR.RISKINFORMATIONDATA COLUMNS 
--            DOCUMENTTYPE    VARCHAR2(80) PATH '/ClientDocument/@DocumentType',
--    		DOCUMENTNUMBER  VARCHAR2(80) PATH '/ClientDocument/@DocumentNUmber') DOCUMENTO),
--        (XMLTABLE('RiskInformation' PASSING UR.RISKINFORMATIONDATA COLUMNS 
--            EFFECTIVEDATE   VARCHAR2(80) PATH '/RiskInformation/@EffectiveDate',
--            LINEOFBUSINESS  VARCHAR2(80) PATH '/RiskInformation/@LineOfBusiness',
--            PRODUCTCODE     VARCHAR2(80) PATH '/RiskInformation/@ProductCode') DATAPOLICY)
--WHERE REPLACE (REPLACE (REPLACE (DOCUMENTO.DOCUMENTNUMBER,'-',''),' ',''),'.','')= REPLACE (REPLACE (REPLACE ('25664715','-',''),' ',''),'.','')
--   AND DATAPOLICY.LINEOFBUSINESS  = 6
--   AND DATAPOLICY.PRODUCTCODE = 6800
--   AND DATAPOLICY.LINEOFBUSINESS  =UC.LINEOFBUSINESS
--   AND DATAPOLICY.PRODUCTCODE = UC.PRODUCT


SELECT CE.SCLIENT, CE.SCUMUL_CODE, MC.SEMAIL 
FROM INSUDB.CERTIFICAT CE 
	JOIN INSUDB.MAIL_CLI MC
ON CE.SCLIENT = MC.SCLIENT


