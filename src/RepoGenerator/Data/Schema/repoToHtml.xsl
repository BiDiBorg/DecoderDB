<?xml version="1.0" encoding="UTF-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:fw="http://www.bidib.org/schema/firmwareRepo">
	<xsl:output method="html"/>
	<xsl:template match="/">
		<html>
			<body>
				<h1>Firmware Repo</h1>
				<xsl:apply-templates/>
			</body>
		</html>
	</xsl:template>
	<xsl:template match="fw:ManufacturerFirmwareInfo">
		<div>Info</div>
		<xsl:apply-templates select="fw:Manufacturer"/>
	</xsl:template>
	<xsl:template match="fw:Manufacturer">
		<h2>
			<xsl:value-of select="fw:Name"/>
		</h2>
	</xsl:template>
	<xsl:template match="fw:Products/fw:ProductInfo">
		<xsl:if test="count(fw:Firmwares/fw:FirmwareInfo) &gt; 0">
			<h3>
				<xsl:value-of select="fw:Name"/> (<xsl:value-of select="fw:Id"/>)</h3>
			<xsl:apply-templates select="fw:Firmwares/fw:FirmwareInfo"/>
			<hr/>
		</xsl:if>
	</xsl:template>
	<xsl:template match="fw:Firmwares/fw:FirmwareInfo">
		<div>
			<span style="margin: 0px 2px;">
				<xsl:value-of select="fw:ReleaseDateValue"/>
			</span>
			<span style="margin: 0px 2px;">
				<a href="{fw:Link}">
					<xsl:value-of select="fw:Version"/>
				</a>
			</span>
			<span style="margin: 0px 2px;">
				
			</span>
			<span>
				<xsl:value-of select="fw:StateType"/>
			</span>
		</div>
	</xsl:template>
	<xsl:template match="fw:Blacklist"/>
</xsl:stylesheet>