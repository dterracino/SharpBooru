<?php

function enc_ctp($str) { return mb_convert_encoding($str, "UTF-8", "UTF-16"); }
function enc_ptc($str) { return mb_convert_encoding($str, "UTF-16", "UTF-8"); }

function get_mime($buffer)
{
        $finfo = finfo_open(FILEINFO_MIME_TYPE);
        $mime = finfo_buffer($finfo, $buffer);
        finfo_close($finfo);
        return $mime;
}

function cache_headers($seconds)
{
                header("Cache-Control: public, max-age=" . $seconds, true);
                header("Expires: " . gmdate("D, d M Y H:i:s", time() + $seconds) . " GMT");
                header_remove("Pragma");
}

function etag_buffer($data)
{
	$etag = dechex(crc32($data));
	header("ETag: " . $etag);
	return $etag;
}

function etag_check($etag)
{
	if (isset($_SERVER["HTTP_IF_NONE_MATCH"]))
		if($_SERVER["HTTP_IF_NONE_MATCH"] == $etag)
		{
			http_response_code(304);
			return true;
		}
	return false;
}

?>
