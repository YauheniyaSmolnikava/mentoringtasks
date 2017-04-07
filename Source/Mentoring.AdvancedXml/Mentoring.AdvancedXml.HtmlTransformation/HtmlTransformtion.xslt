<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:msxsl="urn:schemas-microsoft-com:xslt"
                xmlns:ct="http://library.by/catalog"
                exclude-result-prefixes="msxsl"
>
  <xsl:output method="html" indent="yes"/>

  <xsl:template match="/">
    <xsl:for-each select="ct:catalog/ct:book/ct:genre[not(.=preceding::*)]">
      <xsl:apply-templates select="/ct:catalog">
        <xsl:with-param name="genre" select="."/>
      </xsl:apply-templates>
    </xsl:for-each>
    <h1>
      Total: <xsl:value-of select="count(ct:catalog/ct:book)"/>
    </h1>
  </xsl:template>

  <xsl:template match="ct:catalog">
    <xsl:param name="genre"></xsl:param>
    <html>
      <h1>
        Current funds by genre "<xsl:value-of select="$genre"/>"
      </h1>
      <table>
        <thead>
          <tr>
            <th>Author</th>
            <th>Title</th>
            <th>Publishing Date</th>
            <th>Registration Date</th>
          </tr>
        </thead>
        <tbody>
          <xsl:apply-templates select="ct:book[ct:genre=$genre]"/>
        </tbody>
        <tfoot>
          <tr>
            <th colspan="3">Total:</th>
            <th>
              <xsl:value-of select="count(ct:book[ct:genre=$genre])"/>
            </th>
          </tr>
        </tfoot>
      </table>
    </html>
  </xsl:template>

  <xsl:template match="ct:book">
    <tr>
      <td>
        <xsl:value-of select="ct:author/text()"/>
      </td>
      <td>
        <xsl:value-of select="ct:title/text()"/>
      </td>
      <td>
        <xsl:value-of select="ct:publish_date/text()"/>
      </td>
      <td>
        <xsl:value-of select="ct:registration_date/text()"/>
      </td>
    </tr>
  </xsl:template>

  <xsl:template match="text() | @*"/>
</xsl:stylesheet>
