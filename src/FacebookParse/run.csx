#r "Newtonsoft.Json"

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;

public static void Run(string item, TraceWriter log, ICollector<object> output)
{
    log.Info($"FB Item : {item}");
    // TODO: Create mapping from json to different types? 
    dynamic i = JObject.Parse(item);
    
    // If its not from the pages feed don't process it!
    if (i.@object == "page")
    {
        foreach (var entry in i.entry)
        {
            foreach (var change in entry.changes)
            {
                // Depending on the field? Map a certain way!
                if (change.@field == "feed")
                {
                    // Used for sorting into collections 
                    // If the object is a post/photo/status/video that has a unique FB id use that as the Id otherwise don't set
                    string type = change.value.item;
                    string id = null;
                    switch (type)
                    {
                        case "like":
                            // Likes go into reaction collection!
                            // No id set!
                            type = "reaction";
                            break;
                        case "comment":
                            // Comments go into the comment collection!
                            // Id is the commentId!
                            type = change.value.item;
                            id = change.value.comment_id;
                            break;
                         case "share":
                            // Comments go into the comment collection!
                            // Id is the commentId!
                            type = change.value.item;
                            id = change.value.share_id;
                            break;
                        case "post":
                            // Post go into either userpost or pagepost (if the page and user id are the same) collection
                            // Id is the postid!
                            type = change.value.sender_id == entry.id ? "pagepost" : "userpost";
                            id = change.value.post_id;
                            break;
                        case "video":
                            // Post go into either userpost or pagepost (if the page and user id are the same) collection
                            // Id is the postid!
                            type = change.value.sender_id == entry.id ? "pagepost" : "userpost";
                            id = change.value.post_id;
                            break;
                        case "photo":
                            // Post go into either userpost or pagepost (if the page and user id are the same) collection
                            // Id is the postid!
                            type = change.value.sender_id == entry.id ? "pagepost" : "userpost";
                            id = change.value.post_id;
                            break;
                        case "status":
                            // Post go into either userpost or pagepost (if the page and user id are the same) collection
                            // Id is the postid!
                            type = change.value.sender_id == entry.id ? "pagepost" : "userpost";
                            id = change.value.post_id;
                            break;
                        default:
                            break;
                    }
                    var record = new
                    {
                        // The value of the change sent by facebook (For Testing) TODO: Remove
                        metadata = change.value,

                        type = type,
                        id = id,
                        pageId = entry.id,
                        // The id of the post or comment the object belongs to
                        parentId = change.value.parent_id,                
                        // The FB name of the user who made the change
                        senderName = change.value.sender_name,
                        // The FB Id of the user who made the change
                        senderId = change.value.sender_id,
                        // The type of change (edit, add, etc.)
                        verb = change.value.verb,
                        // What the change was to (comment, post, like, reaction etc.)
                        item = change.value.item,
                        // The time the change was made
                        createdTime = change.value.created_time,
                        // The Id of the photo 
                        photoId = change.value.photo_id,
                        // The Id of the Share 
                        shareId = change.value.share_id,
                        // The Id of the photo 
                        link = change.value.link,
                        // The Id of the comment (To be used as ID if the item is a comment)
                        commentId = change.value.comment_id,
                        // the Id of the post (to be used as ID if the item is a post/photo/video)
                        postId = change.value.post_id,
                        // The value of the message (present for comments and posts etc.)
                        message = change.value.message,
                        // Whether or not the post is visible to the public
                        isHidden = change.value.is_hidden,
                        recipientId = change.value.recipient_id,
                        // The reaction type
                        reactionType = change.value.item == "like" ? "like" : change.value.reaction_type, // Should this be overridden
                        // 0 means unpublished, 1 means published
                        published = change.value.published
                    };

                    
                    log.Info("Record : "+record);
                    output.Add(record);
                    
                }
                /* 
                else if (change.@field == "conversations")
                {
                    var record = new 
                    {
                        // The value of the change sent by facebook (For Testing) TODO: Remove
                        metadata = change.value,

                        pageId = entry.id,
                        threadId = change.value.thread_id,
                        type = "conversations"
                    };
                    log.Info("Record : "+record);
                    var obj = new {
                        collectionName = record.type,
                        record = record
                    };
                    output.Add(obj);
                }*/
            }
        }
    }
}
