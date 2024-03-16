using System.Text.Encodings.Web;

namespace flasher
{
    internal static class HtmlGen
    {
        public static string GetHtml(string title, IReadOnlyList<string> jpgImagesBase64, string source)
        {
            title = HtmlEncoder.Default.Encode(title);

            var images = string.Join(",\n        ", jpgImagesBase64.Select(jpg => $"\"data:image/jpeg;base64, {jpg}\""));

            return $$"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
                <meta charset="UTF-8">
                <meta http-equiv="X-UA-Compatible" content="IE=edge">
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <title>Flash Cards - {{title}}</title>
                <style>
                    body {
                        margin: 0;
                    }
                    .split-container {
                        display: flex;
                        height: 100vh; /* Set the container height to full viewport height */
                    }
                    .image-half {
                        flex: 1; /* Each image takes half of the available width */
                        overflow: hidden; /* Hide any overflow content */
                    }
                    .image {
                        width: 100%;
                        height: 100%;
                        object-fit: contain; 
                    }
                   .hidden-image {
                        display: none; /* Initially hide the second image */
                    }

                    .input-container {
                        display: flex;
                        gap: 10px; /* Add some spacing between the input boxes */
                    }
                    .input-box {
                        flex: 1; /* Expand to fill available space */
                        max-width: 40px;
                    }    
                    .go-button {
                        padding: 5px 10px;
                        background-color: #0074D9;
                        color: #FFFFFF;
                        border: none;
                        cursor: pointer;
                    }            

                </style>
            </head>
            <body>
                <div class="input-container">
                    <label class="input-label" for="start">Start:</label>
                    <input class="input-box" type="number" id="start" name="start" min="0" value="1">
                    <label class="input-label" for="end">End:</label>
                    <input class="input-box" type="number" id="end" name="end" min="0">
                    <button class="go-button" onclick="go()">Go</button>
                    <a href="file:///{{source}}" target="_blank">{{title}}</a>
                </div>
                <div class="split-container">
                    <div class="image-half">
                        <img class="image" id="question-image" alt="Question">
                    </div>
                    <div class="image-half">
                        <img class="image hidden-image" id="answer-image" alt="Answer">
                    </div>
                </div>

                <script>
            	var imageListStart = 0;
            	var sourceImages = [
                    {{images}}
                ];

                function pairConsecutiveItems(arr, start, end) {
                    const pairedList = [];
                    for (let i = start; i < end; i += 2) {
                        if (i + 1 < end) {
                            pairedList.push([arr[i], arr[i + 1]]);
                        }
                    }
                    return pairedList;
                }  

                function shuffleArray(arr) {
                    for (let i = arr.length - 1; i > 0; i--) {
                        // Generate a random index from 0 to i (inclusive)
                        const randomIndex = Math.floor(Math.random() * (i + 1));

                        // Swap elements at randomIndex and i
                        [arr[i], arr[randomIndex]] = [arr[randomIndex], arr[i]];
                    }
                    return arr;
                }    

            	var imageRange = 0;
                var phase = "question"
                const elQuestion = document.getElementById("question-image");
                const elAnswer = document.getElementById("answer-image");
                const answerClassList = elAnswer.classList;
                var i = imageListStart;

                function go() {
                    const start = document.getElementById("start");
                    const end = document.getElementById("end");

                    imageListStart = start.value - 1;
                    var endValue = end.value || sourceImages.length;

                    imageList = pairConsecutiveItems(sourceImages, imageListStart, endValue);
                    imageList = shuffleArray(imageList);
                    imageRange = imageList.length;

                    // todo: check limits:
                    answerClassList.add("hidden-image");
                    phase = "question";

                    i = 0;
                    elQuestion.src = imageList[i][0];
                    elAnswer.src = imageList[i][1];

                    start.focus();

                    console.log(`Start value: ${imageListStart}, End: ${endValue}, Range: ${imageRange}, List Len: ${imageList.length}`);
                }

                go();

                // Listen for the Enter key press
                document.addEventListener("keydown", function(event) {
                    if (event.key === "Enter") {
                        if (phase == "question") {
                            // Show the answer
                            answerClassList.remove("hidden-image");
                            phase = "answer";
                        } else {
                            // Move to next question (and hide answer)                     
                            i++;
                            if (i > imageList.length - 1) {
                                i = 0;
                            }
                            console.log(`i: ${i}`);
                            elQuestion.src = imageList[i][0];
                            elAnswer.src = imageList[i][1];


                            answerClassList.add("hidden-image");
                            phase = "question";
                        }
                    }
                });
                </script>

            </body>
            </html>
            """;
        }
    }
}