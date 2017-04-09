<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="2.0"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                xmlns:ct="http://library.by/catalog"
                exclude-result-prefixes="msxsl"
                xmlns:local="urn:local" 
                extension-element-prefixes="msxsl"
  >
  
  <msxsl:script language="CSharp" implements-prefix="local">
    public string generateGuid()
    {
    return Guid.NewGuid().ToString();
    }
    
    public string convertDateToRfc(string date)
    {
    return DateTime.Parse(date).ToString("ddd, dd MMM yyyy HH':'mm':'ss 'GMT'");
    }
  </msxsl:script>
  <!--<xsl:import href="DateTime.xsl" />-->
  <xsl:output method="rss" indent="yes" />

  <xsl:template match="/ct:catalog">
    <xsl:element name="rss">
      <xsl:attribute name="version">2.0</xsl:attribute>
      <xsl:element name="channel">
        <xsl:element name="title">Books Catalog</xsl:element>
        <xsl:element name="link">http://testcatalog.com</xsl:element>
        <xsl:element name="description">New books list</xsl:element>
        <xsl:apply-templates select="ct:book"/>
      </xsl:element>
    </xsl:element>
  </xsl:template>

  <xsl:template match="ct:book">
    <xsl:element name="item">
      <xsl:element name="title">
        <xsl:value-of select="ct:title/text()"/>
      </xsl:element>
      <xsl:element name="link">
        <xsl:call-template name="constructLink">
          <xsl:with-param name="isbn">
            <xsl:value-of select="ct:isbn"/>
          </xsl:with-param>
          <xsl:with-param name="genre">
            <xsl:value-of select="ct:genre"/>
          </xsl:with-param>
        </xsl:call-template>
      </xsl:element>
      <xsl:element name="description">
        <xsl:value-of select="ct:description/text()"/>
      </xsl:element>
      <xsl:element name="pubDate">
        <xsl:value-of select="local:convertDateToRfc(ct:registration_date/text())"/>
      </xsl:element>
      <xsl:element name="guid">
        <xsl:attribute name="isPermaLink">false</xsl:attribute>
        <xsl:value-of select="local:generateGuid()"/>
      </xsl:element>
    </xsl:element>
  </xsl:template>

  <xsl:template name="constructLink">
    <xsl:param name="isbn"></xsl:param>
    <xsl:param name="genre"></xsl:param>
    <xsl:choose>
      <xsl:when test="string-length($isbn) != 0 and $genre='Computer'">
        http://my.safaribooksonline.com/<xsl:value-of select="$isbn"/>/
      </xsl:when>
      <xsl:otherwise>
        http://my.safaribooksonline.com/
      </xsl:otherwise>
    </xsl:choose>
  </xsl:template>



  <xsl:template match="text() | @*"/>
</xsl:stylesheet>
