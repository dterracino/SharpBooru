window.onload = function () {
	mimg = document.getElementById("mimg");
	mimg.ondblclick = function () {
		if (mimg.style.maxWidth == "none") {
			mimg.style.maxWidth = "800px";
			mimg.style.maxHeight = "800px";
		} else {
			mimg.style.maxWidth = "none";
			mimg.style.maxHeight = "none";
		}
	}
}