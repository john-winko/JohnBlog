// Used on Post.Create and Post.Edit views
function AddTag() {
    var tagEntry = document.getElementById("TagEntry");
    let list = document.getElementById("TagList");
    list.innerHTML += `
    <li class="list-group-item">
        <button type="button" onclick="parentElement.remove();"><i class="bi bi-x-circle"></i></button>
        #${tagEntry.value}
        <input type='hidden' name='tagEntries[]' value='${tagEntry.value}'/>    
        </li>`;
    tagEntry.value = "";
}
         