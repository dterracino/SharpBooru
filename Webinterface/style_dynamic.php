<?php
header("Content-Type: text/css");

require_once("helper.php");
require_once("config.php");

cache_headers(12 * 3600);

?>

div.thumb img {
    max-width: <?php echo $thumb_size; ?>px;
    max-height: <?php echo $thumb_size; ?>px;
    vertical-align: middle;
}

div.thumb {
    width: <?php echo $thumb_size; ?>px;
    height: <?php echo $thumb_size; ?>px;
    float: left;
    margin: 6px;
    line-height: <?php echo $thumb_size; ?>px;
    text-align: center;
}
