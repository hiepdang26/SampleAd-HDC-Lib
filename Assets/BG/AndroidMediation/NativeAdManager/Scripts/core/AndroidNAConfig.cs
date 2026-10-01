using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class AndroidNAConfig
{
    [SerializeField] private string[] ids;
    [SerializeField] private bool autoReload;
	
	public string[] Ids => ids;

	public AndroidNAConfig(string[] ids, bool autoReload = true)
	{
		this.ids = ids;
		this.autoReload = autoReload;
	}
}

[Serializable]
public sealed class NativeClickAssetOptions : IEquatable<NativeClickAssetOptions>
{
    public bool cta = true;
    public bool headline = true;
    public bool body = true;
    public bool description = true;
    public bool icon = true;
    public bool advertiser = true;
    public bool media = true;
    public bool mediaImage = true;
    public bool mediaVideo = true;

    public NativeClickAssetOptions()
    {
    }

    public NativeClickAssetOptions(
        bool cta,
        bool headline,
        bool body,
        bool description,
        bool icon,
        bool advertiser,
        bool media,
        bool mediaImage,
        bool mediaVideo)
    {
        this.cta = cta;
        this.headline = headline;
        this.body = body;
        this.description = description;
        this.icon = icon;
        this.advertiser = advertiser;
        this.media = media;
        this.mediaImage = mediaImage;
        this.mediaVideo = mediaVideo;
    }

    public bool Equals(NativeClickAssetOptions other)
    {
        return other != null &&
               cta == other.cta &&
               headline == other.headline &&
               body == other.body &&
               description == other.description &&
               icon == other.icon &&
               advertiser == other.advertiser &&
               media == other.media &&
               mediaImage == other.mediaImage &&
               mediaVideo == other.mediaVideo;
    }

    public override bool Equals(object obj)
    {
        return obj is NativeClickAssetOptions other && Equals(other);
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(cta);
        hashCode.Add(headline);
        hashCode.Add(body);
        hashCode.Add(description);
        hashCode.Add(icon);
        hashCode.Add(advertiser);
        hashCode.Add(media);
        hashCode.Add(mediaImage);
        hashCode.Add(mediaVideo);
        return hashCode.ToHashCode();
    }
}

[Serializable]
public sealed class NativeAssetVisibilityOptions : IEquatable<NativeAssetVisibilityOptions>
{
    public bool cta = true;
    public bool headline = true;
    public bool body = true;
    public bool description = true;
    public bool icon = true;
    public bool advertiser = true;
    public bool media = true;
    public bool mediaImage = true;
    public bool mediaVideo = true;
    public bool starRating = true;
    public bool store = true;
    public bool price = true;

    public NativeAssetVisibilityOptions()
    {
    }

    public NativeAssetVisibilityOptions(
        bool cta,
        bool headline,
        bool body,
        bool description,
        bool icon,
        bool advertiser,
        bool media,
        bool mediaImage,
        bool mediaVideo,
        bool starRating,
        bool store,
        bool price)
    {
        this.cta = cta;
        this.headline = headline;
        this.body = body;
        this.description = description;
        this.icon = icon;
        this.advertiser = advertiser;
        this.media = media;
        this.mediaImage = mediaImage;
        this.mediaVideo = mediaVideo;
        this.starRating = starRating;
        this.store = store;
        this.price = price;
    }

    public bool Equals(NativeAssetVisibilityOptions other)
    {
        return other != null &&
               cta == other.cta &&
               headline == other.headline &&
               body == other.body &&
               description == other.description &&
               icon == other.icon &&
               advertiser == other.advertiser &&
               media == other.media &&
               mediaImage == other.mediaImage &&
               mediaVideo == other.mediaVideo &&
               starRating == other.starRating &&
               store == other.store &&
               price == other.price;
    }

    public override bool Equals(object obj)
    {
        return obj is NativeAssetVisibilityOptions other && Equals(other);
    }

    public override int GetHashCode()
    {
        var hashCode = new HashCode();
        hashCode.Add(cta);
        hashCode.Add(headline);
        hashCode.Add(body);
        hashCode.Add(description);
        hashCode.Add(icon);
        hashCode.Add(advertiser);
        hashCode.Add(media);
        hashCode.Add(mediaImage);
        hashCode.Add(mediaVideo);
        hashCode.Add(starRating);
        hashCode.Add(store);
        hashCode.Add(price);
        return hashCode.ToHashCode();
    }
}
